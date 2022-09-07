using System.Linq;
using OggVorbisEncoder.Setup;
using OggVorbisEncoder.Setup.Templates;

namespace OggVorbisEncoder
{

public class VorbisInfo
{
    private static readonly Mode[] m_ModeTemplate = { new Mode(0, 0, 0, 0), new Mode(1, 0, 0, 1) };

    private VorbisInfo(
        CodecSetup _CodecSetup,
        int _Channels,
        int _SampleRate,
        int _BITRateNominal)
    {
        CodecSetup = _CodecSetup;
        Channels = _Channels;
        SampleRate = _SampleRate;
        BitRateNominal = _BITRateNominal;
    }

    public int Channels { get; }

    public int SampleRate { get; }

    public int BitRateNominal { get; }

    public CodecSetup CodecSetup { get; }

    public static VorbisInfo InitVariableBitRate(int _Channels, int _SampleRate, float _BaseQuality)
    {
        var encodeSetup = GetEncodeSetup(_Channels, _SampleRate, _BaseQuality);
        var codecSetup = new CodecSetup(encodeSetup);
        var template = encodeSetup.Template;

        // choose Block sizes from configured sizes as well as paying
        // attention to long_Block_p and short_Block_p.  If the configured
        // short and long Blocks are the same length, we set long_Block_p
        // and unset short_Block_p 
        BlockSizeSetup(
            codecSetup,
            (int)encodeSetup.BaseSetting,
            template.BlockSizeShort,
            template.BlockSizeLong);

        var singleBlock = codecSetup.BlockSizes[0] == codecSetup.BlockSizes[1];

        // floor setup; choose proper floor params.  Allocated on the floor
        // stack in order; if we alloc only long floor, it's 0 
        foreach (var floorMappings in template.FloorMappings)
            FloorSetup(
                codecSetup,
                (int)encodeSetup.BaseSetting,
                template.FloorBooks,
                template.FloorParams,
                floorMappings);

        // setup of [mostly] short Block detection and stereo
        GlobalPsychSetup(
            codecSetup,
            encodeSetup.BaseSetting,
            template.GlobalParams,
            template.GlobalMapping);

        GlobalStereo(
            codecSetup,
            _SampleRate,
            template.StereoModes);

        // basic psych setup and noise normalization 
        PsyParamSetup(
            codecSetup,
            (int)encodeSetup.BaseSetting,
            template.PsyNoiseNormalStart[0],
            template.PsyNoiseNormalPartition[0],
            template.PsyNoiseNormalThreshold,
            0);

        PsyParamSetup(
            codecSetup,
            (int)encodeSetup.BaseSetting,
            template.PsyNoiseNormalStart[0],
            template.PsyNoiseNormalPartition[0],
            template.PsyNoiseNormalThreshold,
            1);

        if (!singleBlock)
        {
            PsyParamSetup(
                codecSetup,
                (int)encodeSetup.BaseSetting,
                template.PsyNoiseNormalStart[1],
                template.PsyNoiseNormalPartition[1],
                template.PsyNoiseNormalThreshold,
                2);

            PsyParamSetup(
                codecSetup,
                (int)encodeSetup.BaseSetting,
                template.PsyNoiseNormalStart[1],
                template.PsyNoiseNormalPartition[1],
                template.PsyNoiseNormalThreshold,
                3);
        }

        // tone masking setup 
        ToneMaskSetup(
            codecSetup,
            encodeSetup.BaseSetting,
            0,
            template.PsyToneMasterAtt,
            template.PsyTone0Decibel,
            template.PsyToneAdjImpulse);

        ToneMaskSetup(
            codecSetup,
            encodeSetup.BaseSetting,
            1,
            template.PsyToneMasterAtt,
            template.PsyTone0Decibel,
            template.PsyToneAdjOther);

        if (!singleBlock)
        {
            ToneMaskSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                2,
                template.PsyToneMasterAtt,
                template.PsyTone0Decibel,
                template.PsyToneAdjOther);

            ToneMaskSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                3,
                template.PsyToneMasterAtt,
                template.PsyTone0Decibel,
                template.PsyToneAdjLong);
        }

        // noise compand setup 
        CompandSetup(
            codecSetup,
            encodeSetup.BaseSetting,
            0,
            template.PsyNoiseCompand,
            template.PsyNoiseCompandShortMapping);

        CompandSetup(
            codecSetup,
            encodeSetup.BaseSetting,
            1,
            template.PsyNoiseCompand,
            template.PsyNoiseCompandShortMapping);

        if (!singleBlock)
        {
            CompandSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                2,
                template.PsyNoiseCompand,
                template.PsyNoiseCompandLongMapping);

            CompandSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                3,
                template.PsyNoiseCompand,
                template.PsyNoiseCompandLongMapping);
        }

        // peak guarding setup  
        PeakSetup(
            codecSetup,
            encodeSetup.BaseSetting,
            0,
            template.PsyToneDecibelSuppress);

        PeakSetup(
            codecSetup,
            encodeSetup.BaseSetting,
            1,
            template.PsyToneDecibelSuppress);

        if (!singleBlock)
        {
            PeakSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                2,
                template.PsyToneDecibelSuppress);

            PeakSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                3,
                template.PsyToneDecibelSuppress);
        }

        // noise bias setup 
        NoiseBiasSetup(
            codecSetup,
            encodeSetup.BaseSetting,
            0,
            template.PsyNoiseDecibelSuppress,
            template.PsyNoiseBiasImpulse,
            template.PsyNoiseGuards);

        NoiseBiasSetup(
            codecSetup,
            encodeSetup.BaseSetting,
            1,
            template.PsyNoiseDecibelSuppress,
            template.PsyNoiseBiasPadding,
            template.PsyNoiseGuards);

        if (!singleBlock)
        {
            NoiseBiasSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                2,
                template.PsyNoiseDecibelSuppress,
                template.PsyNoiseBiasTrans,
                template.PsyNoiseGuards);

            NoiseBiasSetup(
                codecSetup,
                encodeSetup.BaseSetting,
                3,
                template.PsyNoiseDecibelSuppress,
                template.PsyNoiseBiasLong,
                template.PsyNoiseGuards);
        }

        AthSetup(codecSetup, 0);
        AthSetup(codecSetup, 1);

        if (!singleBlock)
        {
            AthSetup(codecSetup, 2);
            AthSetup(codecSetup, 3);
        }

        MapAndResSetup(
            codecSetup,
            _SampleRate,
            _Channels,
            template.Maps);

        var bitRateNominal = GetApproxBitRate(encodeSetup, _Channels);

        return new VorbisInfo(
            codecSetup,
            _Channels,
            _SampleRate,
            bitRateNominal);
    }

    private static void PsyParamSetup(
        CodecSetup _CodecSetup,
        int _EncodeSetupBaseSetting,
        int[] _NoiseNormalStart,
        int[] _NoiseNormalPartition,
        double[] _NoiseNormalThreshold,
        int _Block)
    {
        var psyParam = Psy.PsyInfoTemplate.Clone();
        _CodecSetup.PsyParams.Add(psyParam);
        psyParam.BlockFlag = _Block >> 1;

        psyParam.Normalize = true;
        psyParam.NormalStart = _NoiseNormalStart[_EncodeSetupBaseSetting];
        psyParam.NormalPartition = _NoiseNormalPartition[_EncodeSetupBaseSetting];
        psyParam.NormalThreshold = _NoiseNormalThreshold[_EncodeSetupBaseSetting];
    }

    private static void FloorSetup(
        CodecSetup _CodecSetup,
        int _EncodeSetupBaseSetting,
        IStaticCodeBook[][] _TemplateFloorBooks,
        Floor[] _TemplateFloorParams,
        int[] _TemplateFloorMappings)
    {
        var sourceIndex = _TemplateFloorMappings[_EncodeSetupBaseSetting];
        var clonedFloor = _TemplateFloorParams[sourceIndex].Clone();

        // books 
        int maxClass = -1, maxBook = -1;

        foreach (var partitionClass in clonedFloor.PartitionClass)
            if (partitionClass > maxClass)
                maxClass = partitionClass;

        for (var i = 0; i <= maxClass; i++)
        {
            if (clonedFloor.ClassBook[i] > maxBook)
                maxBook = clonedFloor.ClassBook[i];

            clonedFloor.ClassBook[i] += _CodecSetup.BookParams.Count;

            for (var k = 0; k < 1 << clonedFloor.ClassSubs[i]; k++)
            {
                if (clonedFloor.ClassSubBook[i][k] > maxBook)
                    maxBook = clonedFloor.ClassSubBook[i][k];

                if (clonedFloor.ClassSubBook[i][k] >= 0)
                    clonedFloor.ClassSubBook[i][k] += _CodecSetup.BookParams.Count;
            }
        }

        for (var i = 0; i <= maxBook; i++)
        {
            var bookParam = _TemplateFloorBooks[sourceIndex][i];
            _CodecSetup.BookParams.Add(bookParam);
        }

        _CodecSetup.FloorParams.Add(clonedFloor);
    }

    private static void MapAndResSetup(
        CodecSetup _CodecSetup,
        int _SampleRate,
        int _Channels,
        IMappingTemplate[] _TemplateMaps)
    {
        var encodeSetupBaseSetting = (int)_CodecSetup.EncodeSetup.BaseSetting;
        var map = _TemplateMaps[encodeSetupBaseSetting].Mapping;
        var res = _TemplateMaps[encodeSetupBaseSetting].ResidueTemplate;

        var modes = 2;
        if (_CodecSetup.BlockSizes[0] == _CodecSetup.BlockSizes[1])
            modes = 1;

        for (var i = 0; i < modes; i++)
        {
            _CodecSetup.ModeParams.Add(m_ModeTemplate[i]);
            _CodecSetup.MapParams.Add(map[i].Clone());

            for (var j = 0; j < map[i].SubMaps; j++)
                ResidueSetup(
                    _CodecSetup,
                    _SampleRate,
                    _Channels,
                    map[i].ResidueSubMap[j],
                    i,
                    res[map[i].ResidueSubMap[j]]);
        }
    }

    private static void ResidueSetup(
        CodecSetup _CodecSetup,
        int _SampleRate,
        int _Channels,
        int _Number,
        int _Block,
        IResidueTemplate _ResidueTemplate)
    {
        var residue = _ResidueTemplate.Residue.Clone(
            _ResidueTemplate.ResidueType,
            _ResidueTemplate.Grouping);

        _CodecSetup.ResidueParams.Add(residue);

        // fill in all the books
        FillBooks(_CodecSetup, residue, _ResidueTemplate.BookAux, _ResidueTemplate.BooksBase);

        // lowpass setup/pointlimit 
        var freq = _CodecSetup.EncodeSetup.LowPassKilohertz * 1000;
        var f = _CodecSetup.FloorParams[_Block]; // by convention
        var nyq = _SampleRate / 2.0;
        var blocksize = _CodecSetup.BlockSizes[_Block] >> 1;

        // lowpass needs to be set in the floor and the residue. 
        if (freq > nyq)
            freq = nyq;

        // in the floor, the granularity can be very fine; it doesn't alter
        // the encoding structure, only the samples used to fit the floor approximation 
        f.N = (int)(freq / nyq * blocksize);

        // this res may by limited by the maximum pointlimit of the mode,
        // not the lowpass. the floor is always lowpass limited.
        switch (_ResidueTemplate.LimitType)
        {
            case ResidueLimitType.PointStereo:
                freq = _CodecSetup.PsyGlobalParam.CouplingPerKilohertz[PsyGlobal.PACKET_BLOBS / 2] * 1000;

                if (freq > nyq)
                    freq = nyq;

                break;

            case ResidueLimitType.LowFrequencyEffects:
                freq = 250;
                break;
        }

        // in the residue, we're constrained, physically, by partition
        // boundaries.  We still lowpass 'wherever', but we have to round up
        // here to next boundary, or the vorbis spec will round it *down* to
        // previous boundary in encode/decode
        if (residue.ResidueType == ResidueType.Two)
        {
            // Residue 2 bundles together multiple channels; used by stereo
            // and surround.  Count the channels in use 
            // Multiple maps/submaps can point to the same residue.  In the case
            // of residue 2, they all better have the same number of channels/samples. 
            var ch = 0;
            for (var i = 0; (i < _CodecSetup.MapParams.Count) && (ch == 0); i++)
            {
                var mapping = _CodecSetup.MapParams[i];
                for (var j = 0; (j < mapping.SubMaps) && (ch == 0); j++)
                    if (mapping.ResidueSubMap[j] == _Number) // we found a submap referencing this residue backend 
                        for (var k = 0; k < _Channels; k++)
                            if (mapping.ChannelMuxList[k] == j) // this channel belongs to the submap 
                                ch++;
            }

            // round up only if we're well past
            residue.End = (int)(freq / nyq * blocksize * ch / residue.Grouping + .9) * residue.Grouping;

            // the blocksize and grouping may disagree at the end
            if (residue.End > blocksize * ch)
                residue.End = blocksize * ch / residue.Grouping * residue.Grouping;
        }
        else
        {
            // round up only if we're well past
            residue.End = (int)(freq / nyq * blocksize / residue.Grouping + .9) * residue.Grouping;

            // the blocksize and grouping may disagree at the end 
            if (residue.End > blocksize)
                residue.End = blocksize / residue.Grouping * residue.Grouping;
        }

        if (residue.End == 0)
            residue.End = residue.Grouping; // LFE channel 
    }

    private static void FillBooks(
        CodecSetup _CodecSetup,
        ResidueEntry _R,
        IStaticCodeBook _BookAux,
        IStaticBookBlock _BookBlock)
    {
        for (var i = 0; i < _R.Partitions; i++)
            for (var k = 0; k < 4; k++)
                if ((i < _BookBlock.Books.Length)
                    && (k < _BookBlock.Books[i].Length)
                    && (_BookBlock.Books[i][k] != null))
                    _R.SecondStages[i] |= 1 << k;

        _R.GroupBook = GetOrAddBook(_CodecSetup, _BookAux);

        var booklist = 0;
        for (var i = 0; i < _R.Partitions; i++)
            for (var k = 0; k < 4; k++)
                if ((i < _BookBlock.Books.Length)
                    && (k < _BookBlock.Books[i].Length))
                {
                    var sourceBook = _BookBlock.Books[i][k];
                    if (sourceBook != null)
                    {
                        var bookid = GetOrAddBook(_CodecSetup, sourceBook);
                        _R.BookList[booklist++] = bookid;
                    }
                }
    }

    private static int GetOrAddBook(CodecSetup _CodecSetup, IStaticCodeBook _CodeBook)
    {
        int i;
        for (i = 0; i < _CodecSetup.BookParams.Count; i++)
            if (_CodecSetup.BookParams[i] == _CodeBook)
                return i;

        _CodecSetup.BookParams.Add(_CodeBook);
        return _CodecSetup.BookParams.Count - 1;
    }

    private static void GlobalPsychSetup(
        CodecSetup _CodecSetup,
        double _EncodeSetupTriggerSetting,
        PsyGlobal[] _TemplateGlobalParams,
        double[] _TemplateGlobalMapping)
    {
        var setting = (int)_EncodeSetupTriggerSetting;
        var ds = _EncodeSetupTriggerSetting - setting;

        var sourceIndex = (int)_TemplateGlobalMapping[setting];
        var globalParam = _CodecSetup.PsyGlobalParam = _TemplateGlobalParams[sourceIndex].Clone();

        ds = _TemplateGlobalMapping[setting]
             * (1 - ds) + _TemplateGlobalMapping[setting + 1]
             * ds;

        setting = (int)ds;
        ds -= setting;
        if ((ds <= 0) && (setting > 0))
        {
            setting--;
            ds = 1;
        }

        // interpolate the trigger threshholds 
        for (var i = 0; i < 4; i++)
        {
            globalParam.PreEchoThreshold[i] = (float)
            (_TemplateGlobalParams[setting].PreEchoThreshold[i] * (1 - ds)
             + _TemplateGlobalParams[setting + 1].PreEchoThreshold[i] * ds);

            globalParam.PostEchoThreshold[i] = (float)
            (_TemplateGlobalParams[setting].PostEchoThreshold[i] * (1 - ds)
             + _TemplateGlobalParams[setting + 1].PostEchoThreshold[i] * ds);
        }

        globalParam.AmpMaxAttPerSec = (float)_CodecSetup.EncodeSetup.AmplitudeTrackDbPerSec;
    }

    private static void GlobalStereo(
        CodecSetup _CodecSetup,
        int _SampleRate,
        AdjStereo[] _TemplateStereoModes)
    {
        var setting = (int)_CodecSetup.EncodeSetup.BaseSetting;
        var ds = _CodecSetup.EncodeSetup.BaseSetting - setting;
        var psyGlobal = _CodecSetup.PsyGlobalParam;
        var packetBlobs = PsyGlobal.PACKET_BLOBS;

        if (_TemplateStereoModes != null)
        {
            psyGlobal.CouplingPrePointAmp = _TemplateStereoModes[setting].Pre.ToArray();
            psyGlobal.CouplingPostPointAmp = _TemplateStereoModes[setting].Post.ToArray();

            var kHz = _TemplateStereoModes[setting].Kilohertz[packetBlobs / 2] * (1 - ds)
                      + _TemplateStereoModes[setting + 1].Kilohertz[packetBlobs / 2] * ds;

            for (var i = 0; i < packetBlobs; i++)
            {
                psyGlobal.CouplingPointLimit[0][i] =
                    (int)(kHz * 1000 / _SampleRate * _CodecSetup.BlockSizes[0]);
                psyGlobal.CouplingPointLimit[1][i] =
                    (int)(kHz * 1000 / _SampleRate * _CodecSetup.BlockSizes[1]);
                psyGlobal.CouplingPerKilohertz[i] = (int)kHz;
            }

            kHz = _TemplateStereoModes[setting].LowPassKilohertz[packetBlobs / 2] * (1 - ds)
                  + _TemplateStereoModes[setting + 1].LowPassKilohertz[packetBlobs / 2] * ds;

            for (var i = 0; i < packetBlobs; i++)
            {
                psyGlobal.SlidingLowPass[0][i] = (int)(kHz * 1000 / _SampleRate * _CodecSetup.BlockSizes[0]);
                psyGlobal.SlidingLowPass[1][i] = (int)(kHz * 1000 / _SampleRate * _CodecSetup.BlockSizes[1]);
            }
        }
        else
        {
            for (var i = 0; i < packetBlobs; i++)
            {
                psyGlobal.SlidingLowPass[0][i] = _CodecSetup.BlockSizes[0];
                psyGlobal.SlidingLowPass[1][i] = _CodecSetup.BlockSizes[1];
            }
        }
    }

    private static void BlockSizeSetup(
        CodecSetup _CodecSetup,
        int _Index,
        int[] _TemplateBlockSizeShort,
        int[] _TemplateBlockSizeLong)
    {
        var blockshort = _TemplateBlockSizeShort[_Index];
        var blocklong = _TemplateBlockSizeLong[_Index];
        _CodecSetup.BlockSizes[0] = blockshort;
        _CodecSetup.BlockSizes[1] = blocklong;
    }

    private static void ToneMaskSetup(
        CodecSetup _CodecSetup,
        double _ToneMaskSetting,
        int _Block,
        Att3[] _TemplatePsyToneMasterAtt,
        int[] _TemplatePsyTone0Decibel,
        AdjBlock[] _TemplatePsyToneAdjLong)
    {
        var setting = (int)_ToneMaskSetting;
        var ds = _ToneMaskSetting - setting;

        var psyParam = _CodecSetup.PsyParams[_Block];

        // 0 and 2 are only used by bitmanagement, but there's no harm to always filling the values in here
        psyParam.ToneMasterAtt[0] = (float)(_TemplatePsyToneMasterAtt[setting].Att[0] * (1 - ds)
                                             + _TemplatePsyToneMasterAtt[setting + 1].Att[0] * ds);

        psyParam.ToneMasterAtt[1] = (float)(_TemplatePsyToneMasterAtt[setting].Att[1] * (1 - ds)
                                             + _TemplatePsyToneMasterAtt[setting + 1].Att[1] * ds);

        psyParam.ToneMasterAtt[2] = (float)(_TemplatePsyToneMasterAtt[setting].Att[2] * (1 - ds)
                                             + _TemplatePsyToneMasterAtt[setting + 1].Att[2] * ds);

        psyParam.ToneCenterBoost = (float)(_TemplatePsyToneMasterAtt[setting].Boost * (1 - ds)
                                            + _TemplatePsyToneMasterAtt[setting + 1].Boost * ds);

        psyParam.ToneDecay = (float)(_TemplatePsyToneMasterAtt[setting].Decay * (1 - ds)
                                      + _TemplatePsyToneMasterAtt[setting + 1].Decay * ds);

        psyParam.MaxCurveDecibel = (float)(_TemplatePsyTone0Decibel[setting] * (1 - ds)
                                            + _TemplatePsyTone0Decibel[setting + 1] * ds);

        for (var i = 0; i < PsyInfo.BANDS; i++)
            psyParam.ToneAtt[i] = (float)(_TemplatePsyToneAdjLong[setting].Block[i] * (1 - ds)
                                           + _TemplatePsyToneAdjLong[setting + 1].Block[i] * ds);
    }

    private static void CompandSetup(
        CodecSetup _CodecSetup,
        double _NoiseCompandSetting,
        int _Block,
        CompandBlock[] _TemplatePsyNoiseCompand,
        double[] _TemplatePsyNoiseCompandShortMapping)
    {
        var setting = (int)_NoiseCompandSetting;
        var ds = _NoiseCompandSetting - setting;
        var p = _CodecSetup.PsyParams[_Block];

        ds = _TemplatePsyNoiseCompandShortMapping[setting] * (1 - ds)
             + _TemplatePsyNoiseCompandShortMapping[setting + 1] * ds;

        setting = (int)ds;
        ds -= setting;
        if ((ds <= 0) && (setting > 0))
        {
            setting--;
            ds = 1;
        }

        // interpolate the compander settings 
        for (var i = 0; i < p.NoiseCompand.Length; i++)
            p.NoiseCompand[i] = (float)(_TemplatePsyNoiseCompand[setting].Data[i] * (1 - ds)
                                         + _TemplatePsyNoiseCompand[setting + 1].Data[i] * ds);
    }

    private static void PeakSetup(
        CodecSetup _CodecSetup,
        double _TonePeakLimitSetting,
        int _Block,
        int[] _TemplatePsyToneDecibelSuppress)
    {
        var setting = (int)_TonePeakLimitSetting;
        var ds = _TonePeakLimitSetting - setting;
        var p = _CodecSetup.PsyParams[_Block];

        p.ToneAbsLimit = (float)(_TemplatePsyToneDecibelSuppress[setting] * (1 - ds)
                                  + _TemplatePsyToneDecibelSuppress[setting + 1] * ds);
    }

    private static void NoiseBiasSetup(
        CodecSetup _CodecSetup,
        double _NoiseBiasSetting,
        int _Block,
        int[] _TemplatePsyNoiseDecibelSuppress,
        Noise3[] _TemplatePsyNoiseBiasLong,
        NoiseGuard[] _TemplatePsyNoiseGuards)
    {
        var setting = (int)_NoiseBiasSetting;
        var ds = _NoiseBiasSetting - setting;
        var psyParam = _CodecSetup.PsyParams[_Block];

        psyParam.NoiseMaxSuppress = (float)(_TemplatePsyNoiseDecibelSuppress[setting] * (1 - ds)
                                             + _TemplatePsyNoiseDecibelSuppress[setting + 1] * ds);

        psyParam.NoiseWindowLowMin = _TemplatePsyNoiseGuards[_Block].Low;
        psyParam.NoiseWindowHighMin = _TemplatePsyNoiseGuards[_Block].High;
        psyParam.NoiseWindowFixed = _TemplatePsyNoiseGuards[_Block].Fixed;

        for (var j = 0; j < psyParam.NoiseOffset.Length; j++)
            for (var i = 0; i < psyParam.NoiseOffset[j].Length; i++)
                psyParam.NoiseOffset[j][i] = (float)(_TemplatePsyNoiseBiasLong[setting].Data[j][i] * (1 - ds)
                                                      + _TemplatePsyNoiseBiasLong[setting + 1].Data[j][i] * ds);

        // impulse blocks may take a user specified bias to boost the nominal/high noise encoding depth
        foreach (var noiseOffset in psyParam.NoiseOffset)
        {
            var min = noiseOffset[0] + 6;
            for (var i = 0; i < noiseOffset.Length; i++)
                if (noiseOffset[i] < min)
                    noiseOffset[i] = min;
        }
    }

    private static void AthSetup(CodecSetup _CodecSetup, int _Block)
    {
        var psyParam = _CodecSetup.PsyParams[_Block];

        psyParam.AthAdjAtt = (float)_CodecSetup.EncodeSetup.AthFloatingDecibel;
        psyParam.AthMaxAtt = (float)_CodecSetup.EncodeSetup.AthAbsoluteDecibel;
    }

    private static EncodeSetup GetEncodeSetup(
        int _Channels,
        int _SampleRate,
        float _Quality)
    {
        _Quality += .0000001f;
        if (_Quality >= 1)
            _Quality = .9999f;

        return EncodeSetup.GetBestMatch(
            _Channels,
            _SampleRate,
            _Quality);
    }

    private static int GetApproxBitRate(EncodeSetup _EncodeSetup, int _Channels)
    {
        var template = _EncodeSetup.Template;

        var setting = (int)_EncodeSetup.BaseSetting;
        var ds = _EncodeSetup.BaseSetting - setting;

        if (template.SampleRateMapping == null)
            return -1;

        return (int)((template.SampleRateMapping[setting] * (1 - ds)
            + template.SampleRateMapping[setting + 1] * ds) * _Channels);
    }
}
}
