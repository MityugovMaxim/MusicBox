using System;
using System.Threading.Tasks;
using OggVorbisEncoder.Lookups;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder
{

/// <summary>
///     Buffers the current vorbis audio analysis/synthesis state.
///     The DSP state belongs to a specific logical bitstream
/// </summary>
public class ProcessingState
{
    // .345 is a hack; the original ToDecibel estimation used on IEEE 754
    // compliant machines had an error that returned dB values about a third
    // of a decibel too high.  The error was harmless because tunings
    // implicitly took that into account.  However, fixing the error
    // in the estimator requires changing all the tunings as well.
    // For now, it's easier to sync things back up here, and
    // recalibrate the tunings in the next major model upgrade. 
    private const float DecibelOffset = .345f;
    private readonly LookupCollection m_Lookups;
    private readonly float[][] m_PCM;

    private readonly VorbisInfo m_VorbisInfo;
    private readonly int[] m_Window;

    private int m_CenterWindow;
    private int m_CurrentWindow;
    private int m_EofFlag;
    private int m_GranulePosition;
    private int m_LastWindow;
    private int m_NextWindow;
    private int m_PCMCurrent;
    private bool m_PreExtrapolated;
    private int m_Sequence = 3; // compressed audio packets start after the headers with sequence number 3 

    private ProcessingState(
        VorbisInfo _VorbisInfo,
        LookupCollection _Lookups,
        float[][] _PCM,
        int[] _Window,
        int _CenterWindow)
    {
        m_VorbisInfo = _VorbisInfo;
        m_Lookups = _Lookups;
        m_PCM = _PCM;
        m_Window = _Window;
        m_CenterWindow = _CenterWindow;
    }

    /// <summary>
    ///     Writes the provided data to the pcm buffer
    /// </summary>
    /// <param name="_Data">The data to write in an array of dimensions: channels * length</param>
    /// <param name="_Length">The number of elements to write</param>
    /// <param name="_ReadOffset">Where to start reading from.</param>
    public void WriteData(float[][] _Data, int _Length, int _ReadOffset = 0)
    {
        if (_Length <= 0)
            return;

        EnsureBufferSize(_Length);

        for (var i = 0; i < _Data.Length; ++i)
            Array.Copy(_Data[i], _ReadOffset, m_PCM[i], m_PCMCurrent, _Length);

        var vi = m_VorbisInfo;
        var ci = vi.CodecSetup;
        m_PCMCurrent += _Length;

        // we may want to reverse extrapolate the beginning of a stream too... in case we're beginning on a cliff! 
        // clumsy, but simple.  It only runs once, so simple is good. 
        if (!m_PreExtrapolated && (m_PCMCurrent - m_CenterWindow > ci.BlockSizes[1]))
            PreExtrapolate();
    }

    /// <summary>
    ///     Writes the provided data to the pcm buffer
    /// </summary>
    /// <param name="_Data">The data to write in an array of dimensions: channels * length</param>
    /// <param name="_Length">The number of elements to write</param>
    public void WriteData(float[] _Data, int _Length)
    {
        if (_Length <= 0)
            return;

        EnsureBufferSize(_Length);
        
        var vi = m_VorbisInfo;
        
        int channels = vi.Channels;

        for (int i = 0; i < _Data.Length; i++)
        {
            int channel = i % channels;
            int index   = i / channels;
            m_PCM[channel][m_PCMCurrent + index] = _Data[i];
        }
        
        var ci = vi.CodecSetup;
        m_PCMCurrent += _Length;

        // we may want to reverse extrapolate the beginning of a stream too... in case we're beginning on a cliff! 
        // clumsy, but simple.  It only runs once, so simple is good. 
        if (!m_PreExtrapolated && (m_PCMCurrent - m_CenterWindow > ci.BlockSizes[1]))
            PreExtrapolate();
    }

    public Task WriteDataAsync(float[] _Data, int _Length)
    {
        return Task.Run(() => WriteData(_Data, _Length));
    }

    public void WriteEndOfStream()
    {
        var ci = m_VorbisInfo.CodecSetup;

        const int order = 32;
        var lpc = new float[order];

        // if it wasn't done earlier (very short sample) 
        if (!m_PreExtrapolated)
            PreExtrapolate();

        // We're encoding the end of the stream.  Just make sure we have [at least] a few full blocks of zeroes at the end. 
        // actually, we don't want zeroes; that could drop a large amplitude off a cliff, creating spread spectrum noise that will
        // suck to encode.  Extrapolate for the sake of cleanliness.
        EnsureBufferSize(ci.BlockSizes[1] * 3);

        m_EofFlag = m_PCMCurrent;
        m_PCMCurrent += ci.BlockSizes[1] * 3;

        for (var channel = 0; channel < m_PCM.Length; channel++)
            if (m_EofFlag > order * 2)
            {
                // extrapolate with LPC to fill in
                // make and run a predictor filter 
                var n = m_EofFlag;
                if (n > ci.BlockSizes[1])
                    n = ci.BlockSizes[1];

                PopulateLpcFromPcm(lpc, m_PCM[channel], m_EofFlag - n, n, order);
                UpdatePcmFromLpcPredict(lpc, m_PCM[channel], m_EofFlag, order, m_PCMCurrent - m_EofFlag);
            }
            else
            {
                // not enough data to extrapolate (unlikely to happen due to guarding the overlap, 
                // but bulletproof in case that assumption goes away). zeroes will do.
                Array.Clear(m_PCM[channel], m_EofFlag, m_PCMCurrent - m_EofFlag);
            }
    }

    private static void UpdatePcmFromLpcPredict(
        float[] _LpcCoeff,
        float[] _Data,
        int _Offset,
        int _M,
        int _N)
    {
        var work = new float[_M + _N];

        for (var i = 0; i < _M; i++)
            work[i] = _Data[_Offset - _M + i];

        for (var i = 0; i < _N; i++)
        {
            int o = i, p = _M;
            float y = 0;

            for (var j = 0; j < _M; j++)
                y -= work[o++] * _LpcCoeff[--p];

            _Data[_Offset + i] = work[o] = y;
        }
    }

    private static void PopulateLpcFromPcm(float[] _Lpci, float[] _Data, int _Offset, int _N, int _M)
    {
        var aut = new double[_M + 1];
        var lpc = new double[_M];

        // autocorrelation, p+1 lag coefficients 
        var j = _M + 1;
        while (j-- > 0)
        {
            double d = 0; // double needed for accumulator depth 
            for (var i = j; i < _N; i++)
                d += (double)_Data[_Offset + i]
                     * _Data[_Offset + i - j];

            aut[j] = d;
        }

        // Generate lpc coefficients from autocorr values 
        // set our noise floor to about -100dB 
        var error = aut[0] * (1.0 + 1e-10);
        var epsilon = 1e-9 * aut[0] + 1e-10;

        for (var i = 0; i < _M; i++)
        {
            var r = -aut[i + 1];

            if (error < epsilon)
            {
                Array.Clear(lpc, i, _M - i);
                break;
            }

            // Sum up ampPtr iteration's reflection coefficient; note that in
            // Vorbis we don't save it.  If anyone wants to recycle ampPtr code
            // and needs reflection coefficients, save the results of 'r' from
            // each iteration. 
            for (j = 0; j < i; j++)
                r -= lpc[j] * aut[i - j];

            r /= error;

            // Update LPC coefficients and total error 
            lpc[i] = r;
            for (j = 0; j < i / 2; j++)
            {
                var tmp = lpc[j];
                lpc[j] += r * lpc[i - 1 - j];
                lpc[i - 1 - j] += r * tmp;
            }

            if ((i & 1) != 0)
                lpc[j] += lpc[j] * r;

            error *= 1.0 - r * r;
        }

        // slightly damp the filter 
        {
            var g = .99;
            var damp = g;
            for (j = 0; j < _M; j++)
            {
                lpc[j] *= damp;
                damp *= g;
            }
        }

        for (j = 0; j < _M; j++)
            _Lpci[j] = (float)lpc[j];
    }

    private void PreExtrapolate()
    {
        const int order = 16;

        m_PreExtrapolated = true;

        if (m_PCMCurrent - m_CenterWindow <= order * 2)
            return;

        var lpc = new float[order];
        var work = new float[m_PCMCurrent];

        for (var channel = 0; channel < m_PCM.Length; channel++)
        {
            // need to run the extrapolation in reverse! 
            for (var j = 0; j < m_PCMCurrent; j++)
                work[j] = m_PCM[channel][m_PCMCurrent - j - 1];

            // prime as above 
            PopulateLpcFromPcm(lpc, work, 0, m_PCMCurrent - m_CenterWindow, order);

            // run the predictor filter 
            UpdatePcmFromLpcPredict(lpc, work, m_PCMCurrent - m_CenterWindow, order, m_CenterWindow);

            for (var j = 0; j < m_PCMCurrent; j++)
                m_PCM[channel][m_PCMCurrent - j - 1] = work[j];
        }
    }

    public void EnsureBufferSize(int _Needed)
    {
        var pcmStorage = m_PCM[0].Length;
        if (m_PCMCurrent + _Needed < pcmStorage)
            return;

        pcmStorage = m_PCMCurrent + _Needed * 2;

        for (var i = 0; i < m_PCM.Length; i++)
        {
            var buffer = m_PCM[i];
            Array.Resize(ref buffer, pcmStorage);
            m_PCM[i] = buffer;
        }
    }

    public bool PacketOut(out OggPacket _Packet)
    {
        _Packet = null;

        // Have we started?
        if (!m_PreExtrapolated)
            return false;

        // Are we done?
        if (m_EofFlag == -1)
            return false;

        var codecSetup = m_VorbisInfo.CodecSetup;

        // By our invariant, we have lW, W and centerW set.  Search for
        // the next boundary so we can determine nW (the next window size)
        // which lets us compute the shape of the current block's window
        // we do an envelope search even on a single blocksize; we may still
        // be throwing more bits at impulses, and envelope search handles
        // marking impulses too. 
        var testWindow =
            m_CenterWindow +
            codecSetup.BlockSizes[m_CurrentWindow] / 4 +
            codecSetup.BlockSizes[1] / 2 +
            codecSetup.BlockSizes[0] / 4;

        var bp = m_Lookups.EnvelopeLookup.Search(m_PCM, m_PCMCurrent, m_CenterWindow, testWindow);

        if (bp == -1)
        {
            if (m_EofFlag == 0)
                return false; // not enough data currently to search for a full int block

            m_NextWindow = 0;
        }
        else
        {
            m_NextWindow = codecSetup.BlockSizes[0] == codecSetup.BlockSizes[1] ? 0 : bp;
        }

        var centerNext = m_CenterWindow
                         + codecSetup.BlockSizes[m_CurrentWindow] / 4
                         + codecSetup.BlockSizes[m_NextWindow] / 4;

        // center of next block + next block maximum right side.
        var blockbound = centerNext + codecSetup.BlockSizes[m_NextWindow] / 2;

        // Not enough data yet
        if (m_PCMCurrent < blockbound)
            return false;

        // copy the vectors; ampPtr uses the local storage in vb 
        // ampPtr tracks 'strongest peak' for later psychoacoustics
        var n = codecSetup.BlockSizes[m_CurrentWindow] / 2;
        m_Lookups.PsyGlobalLookup.DecayAmpMax(n, m_VorbisInfo.SampleRate);

        var pcmEnd = codecSetup.BlockSizes[m_CurrentWindow];
        var pcm = new float[m_PCM.Length][];
        var beginWindow = m_CenterWindow - codecSetup.BlockSizes[m_CurrentWindow] / 2;

        for (var channel = 0; channel < m_PCM.Length; channel++)
        {
            pcm[channel] = new float[pcmEnd];
            Array.Copy(m_PCM[channel], beginWindow, pcm[channel], 0, pcm[channel].Length);
        }

        // handle eof detection: eof==0 means that we've not yet received EOF eof>0  
        // marks the last 'real' sample in pcm[] eof<0  'no more to do'; doesn't get here 
        var eofFlag = false;
        if (m_EofFlag != 0)
            if (m_CenterWindow >= m_EofFlag)
            {
                m_EofFlag = -1;
                eofFlag = true;
            }

        var data = PerformAnalysis(pcm, pcmEnd);
        _Packet = new OggPacket(data, eofFlag, m_GranulePosition, m_Sequence++);

        if (!eofFlag)
            AdvanceStorageVectors(centerNext);

        return true;
    }

    private void AdvanceStorageVectors(int _CenterNext)
    {
        // advance storage vectors and clean up 
        var newCenterNext = m_VorbisInfo.CodecSetup.BlockSizes[1] / 2;
        var movementW = _CenterNext - newCenterNext;

        if (movementW <= 0)
            return;

        m_Lookups.EnvelopeLookup.Shift(movementW);
        m_PCMCurrent -= movementW;

        for (var channel = 0; channel < m_PCM.Length; channel++)
            Array.Copy(m_PCM[channel], movementW, m_PCM[channel], 0, m_PCMCurrent);

        m_LastWindow = m_CurrentWindow;
        m_CurrentWindow = m_NextWindow;
        m_CenterWindow = newCenterNext;

        if (m_EofFlag != 0)
        {
            m_EofFlag -= movementW;
            if (m_EofFlag <= 0)
                m_EofFlag = -1;

            // do not add padding to end of stream! 
            if (m_CenterWindow >= m_EofFlag)
                m_GranulePosition += movementW - (m_CenterWindow - m_EofFlag);
            else
                m_GranulePosition += movementW;
        }
        else
        {
            m_GranulePosition += movementW;
        }
    }

    private bool MarkEnvelope()
    {
        var ve = m_Lookups.EnvelopeLookup;
        var ci = m_VorbisInfo.CodecSetup;

        var beginW = m_CenterWindow - ci.BlockSizes[m_CurrentWindow] / 4;
        var endW = m_CenterWindow + ci.BlockSizes[m_CurrentWindow] / 4;

        if (m_CurrentWindow != 0)
        {
            beginW -= ci.BlockSizes[m_LastWindow] / 4;
            endW += ci.BlockSizes[m_NextWindow] / 4;
        }
        else
        {
            beginW -= ci.BlockSizes[0] / 4;
            endW += ci.BlockSizes[0] / 4;
        }

        return ve.Mark(beginW, endW);
    }

    public static ProcessingState Create(VorbisInfo _Info)
    {
        if (_Info == null)
            throw new ArgumentNullException(nameof(_Info));

        var codecSetup = _Info.CodecSetup;

        // initialize the storage vectors. blocksize[1] is small for encode, but the correct size for decode 
        var pcmStorage = codecSetup.BlockSizes[1];

        var pcm = new float[_Info.Channels][];
        for (var i = 0; i < pcm.Length; i++)
            pcm[i] = new float[pcmStorage];

        // Vorbis I uses only window type 0
        var window = new int[2];
        window[0] = Encoding.Log(codecSetup.BlockSizes[0]) - 7;
        window[1] = Encoding.Log(codecSetup.BlockSizes[1]) - 7;

        var centerWindow = codecSetup.BlockSizes[1] / 2;

        var lookups = LookupCollection.Create(_Info);

        return new ProcessingState(
            _Info,
            lookups,
            pcm,
            window,
            centerWindow);
    }

    private byte[] PerformAnalysis(
        float[][] _PCM,
        int _PCMEnd)
    {
        var channels = _PCM.Length;

        var gmdct = new float[channels][];
        var work = new int[channels][];
        var floorPosts = new int[channels][][];
        var localAmpMax = new float[channels];

        var blockType = 0;
        if (m_CurrentWindow != 0)
        {
            if ((m_LastWindow != 0) && (m_NextWindow != 0))
                blockType = 1;
        }
        else
        {
            if (!MarkEnvelope())
                blockType = 1;
        }

        var mapping = m_VorbisInfo.CodecSetup.MapParams[m_CurrentWindow];
        var psyLookup = m_Lookups.PsyLookup[blockType + (m_CurrentWindow != 0 ? 2 : 0)];

        var buffer = new EncodeBuffer();

        TransformPcm(_PCM, _PCMEnd, work, gmdct, localAmpMax);
        ApplyMasks(_PCM, _PCMEnd, mapping, floorPosts, gmdct, psyLookup, localAmpMax);
        Encode(_PCM, _PCMEnd, buffer, mapping, work, floorPosts, psyLookup, gmdct);

        return buffer.GetBytes();
    }

    private void Encode(
        float[][] _PCM,
        int _PCMEnd,
        EncodeBuffer _Buffer,
        Mapping _Mapping,
        int[][] _Work,
        int[][][] _FloorPosts,
        PsyLookup _PsyLookup,
        float[][] _Gmdct)
    {
        var codecSetup = m_VorbisInfo.CodecSetup;
        var channels = _PCM.Length;

        var nonzero = new bool[channels];

        //the next phases are performed once for vbr-only and PACKETBLOB
        //times for bitrate managed modes.

        //1) encode actual mode being used
        //2) encode the floor for each channel, compute coded mask curve/res
        //3) normalize and couple.
        //4) encode residue
        //5) save packet bytes to the packetblob vector

        // iterate over the many masking curve fits we've created 
        var coupleBundle = new int[channels][];
        var zerobundle = new bool[channels];

        const int k = PsyGlobal.PACKET_BLOBS / 2;

        // start out our new packet blob with packet type and mode 
        // Encode the packet type 
        _Buffer.Write(0, 1);
        // Encode the modenumber 
        // Encode frame mode, pre,post windowsize, then dispatch 
        var modeBits = Encoding.Log(codecSetup.ModeParams.Count - 1);
        _Buffer.Write((uint)m_CurrentWindow, modeBits);
        if (m_CurrentWindow != 0)
        {
            _Buffer.Write((uint)m_LastWindow, 1);
            _Buffer.Write((uint)m_NextWindow, 1);
        }

        // encode floor, compute masking curve, sep out residue 
        for (var i = 0; i < channels; i++)
        {
            var submap = _Mapping.ChannelMuxList[i];

            nonzero[i] = m_Lookups.FloorLookup[_Mapping.FloorSubMap[submap]].Encode(
                _Buffer,
                codecSetup.BookParams,
                codecSetup.FullBooks,
                _FloorPosts[i][k],
                _Work[i],
                _PCMEnd,
                codecSetup.BlockSizes[m_CurrentWindow] / 2);
        }

        // our iteration is now based on masking curve, not prequant and
        // coupling.  Only one prequant/coupling step quantize/couple 
        // incomplete implementation that assumes the tree is all depth
        // one, or no tree at all 
        _PsyLookup.CoupleQuantizeNormalize(
            k,
            codecSetup.PsyGlobalParam,
            _Mapping,
            _Gmdct,
            _Work,
            nonzero,
            codecSetup.PsyGlobalParam.SlidingLowPass[m_CurrentWindow][k],
            channels);

        // classify and encode by submap 
        for (var i = 0; i < _Mapping.SubMaps; i++)
        {
            var channelsInBundle = 0;

            var resNumber = _Mapping.ResidueSubMap[i];

            for (var j = 0; j < channels; j++)
                if (_Mapping.ChannelMuxList[j] == i)
                {
                    zerobundle[channelsInBundle] = nonzero[j];
                    coupleBundle[channelsInBundle++] = _Work[j];
                }

            var residue = m_Lookups.ResidueLookup[resNumber];
            var classifications = residue.Class(
                coupleBundle,
                zerobundle,
                channelsInBundle);

            channelsInBundle = 0;
            for (var j = 0; j < channels; j++)
                if (_Mapping.ChannelMuxList[j] == i)
                    coupleBundle[channelsInBundle++] = _Work[j];

            residue.Forward(
                _Buffer,
                _PCMEnd,
                coupleBundle,
                zerobundle,
                channelsInBundle,
                classifications);
        }
    }

    private void TransformPcm(
        float[][] _InputPcm,
        int _PCMEnd,
        int[][] _Work,
        float[][] _Gmdct,
        float[] _LocalAmpMax)
    {
        for (var channel = 0; channel < _InputPcm.Length; channel++)
        {
            _Work[channel] = new int[_PCMEnd / 2];
            _Gmdct[channel] = new float[_PCMEnd / 2];

            var scale = 4f / _PCMEnd;
            var scaleDecibel = scale.ToDecibel() + DecibelOffset;
            var pcm = _InputPcm[channel];

            // window the PCM data 
            ApplyWindow(
                pcm,
                m_LastWindow,
                m_CurrentWindow,
                m_NextWindow);

            // transform the PCM data - only MDCT right now..
            var transform = m_Lookups.TransformLookup[m_CurrentWindow];
            transform.Forward(pcm, _Gmdct[channel]);

            // FFT yields more accurate tonal estimation (not phase sensitive) 
            var ffft = m_Lookups.FftLookup[m_CurrentWindow];
            ffft.Forward(pcm);

            pcm[0] = scaleDecibel
                     + pcm[0].ToDecibel()
                     + DecibelOffset;

            _LocalAmpMax[channel] = _InputPcm[channel][0];

            for (var j = 1; j < _PCMEnd - 1; j += 2)
            {
                var temp = pcm[j] * pcm[j]
                           + pcm[j + 1] * pcm[j + 1];

                var index = (j + 1) >> 1;
                temp = pcm[index] = scaleDecibel + .5f
                                    * temp.ToDecibel()
                                    + DecibelOffset;

                if (temp > _LocalAmpMax[channel])
                    _LocalAmpMax[channel] = temp;
            }

            if (_LocalAmpMax[channel] > 0f)
                _LocalAmpMax[channel] = 0f;
        }
    }

    private void ApplyWindow(
        float[] _PCM,
        int _LastWindow,
        int _Window,
        int _NextWindow)
    {
        _LastWindow = _Window != 0 ? _LastWindow : 0;
        _NextWindow = _Window != 0 ? _NextWindow : 0;

        var windowLastWindow = Block.Windows[m_Window[_LastWindow]];
        var windowNextWindow = Block.Windows[m_Window[_NextWindow]];

        var blockSizes = m_VorbisInfo.CodecSetup.BlockSizes;
        var n = blockSizes[_Window];
        var ln = blockSizes[_LastWindow];
        var rn = blockSizes[_NextWindow];

        var leftbegin = n / 4 - ln / 4;
        var leftend = leftbegin + ln / 2;

        var rightbegin = n / 2 + n / 4 - rn / 4;
        var rightend = rightbegin + rn / 2;

        int i, p;
        for (i = 0; i < leftbegin; i++)
            _PCM[i] = 0f;

        for (p = 0; i < leftend; i++, p++)
            _PCM[i] *= windowLastWindow[p];

        for (i = rightbegin, p = rn / 2 - 1; i < rightend; i++, p--)
            _PCM[i] *= windowNextWindow[p];

        for (; i < n; i++)
            _PCM[i] = 0f;
    }

    private void ApplyMasks(
        float[][] _InputPcm,
        int _PCMEnd,
        Mapping _Mapping,
        int[][][] _FloorPosts,
        float[][] _Gmdct,
        PsyLookup _PsyLookup,
        float[] _LocalAmpMax)
    {
        var pcmEndOver2 = _PCMEnd / 2;
        var noise = new float[pcmEndOver2];
        var tone = new float[pcmEndOver2];

        for (var channel = 0; channel < _InputPcm.Length; channel++)
        {
            // the encoder setup assumes that all the modes used by any
            // specific bitrate tweaking use the same floor 
            var submap = _Mapping.ChannelMuxList[channel];

            var pcm = _InputPcm[channel];
            var logmdct = new Span<float>(pcm, pcmEndOver2, pcm.Length - pcmEndOver2);

            _FloorPosts[channel] = new int[PsyGlobal.PACKET_BLOBS][];

            for (var j = 0; j < pcmEndOver2; j++)
                logmdct[j] = _Gmdct[channel][j].ToDecibel() + DecibelOffset;

            // first step; noise masking.  Not only does 'noise masking'
            // give us curves from which we can decide how much resolution
            // to give noise parts of the spectrum, it also implicitly hands
            // us a tonality estimate (the larger the value in the
            // 'noise_depth' vector, the more tonal that area is) 
            _PsyLookup.NoiseMask(logmdct, noise);

            var globalAmpMax = (int)m_Lookups.PsyGlobalLookup.AmpMax;
            foreach (var ampMax in _LocalAmpMax)
                if (ampMax > globalAmpMax)
                    globalAmpMax = (int)ampMax;

            // second step: 'all the other stuff'; all the stuff that isn't
            // computed/fit for bitrate management goes in the second psy
            // vector.  This includes tone masking, peak limiting and ATH 
            _PsyLookup.ToneMask(
                pcm,
                tone,
                globalAmpMax,
                _LocalAmpMax[channel]);

            // third step; we offset the noise vectors, overlay tone
            //masking.  We then do a floor1-specific line fit.  If we're
            //performing bitrate management, the line fit is performed
            //multiple times for up/down tweakage on demand. 
            _PsyLookup.OffsetAndMix(
                noise,
                tone,
                1,
                pcm,
                _Gmdct[channel],
                logmdct);

            var floor = m_Lookups.FloorLookup[_Mapping.FloorSubMap[submap]];
            _FloorPosts[channel][PsyGlobal.PACKET_BLOBS / 2] = floor.Fit(logmdct, pcm);
        }
    }
}
}
