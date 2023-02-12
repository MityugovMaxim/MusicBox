using System;
using System.Linq;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder.Lookups
{
	public class EnvelopeLookup
	{
		public const int                   ENVELOPE_POST       = 2;
		const        int                   SearchStep         = 64;
		const        int                   WindowLength       = 128;
		const        int                   EnvelopeWindow     = 4;
		const        int                   EnvelopeMinStretch = 2;
		const        int                   EnvelopeMaxStretch = 12; // One third full block
		readonly     EnvelopeBand[]        m_Bands;
		readonly     EnvelopeFilterState[] m_Filters;
		readonly     MdctLookup            m_MdctLookup;

		readonly float[]   m_MdctWindow;
		readonly float     m_MinEnergy;
		readonly PsyGlobal m_PsyGlobal;
		int                m_Current;
		int                m_CurrentMark;
		int                m_Cursor;

		int[] m_Mark;
		int   m_Stretch;

		public EnvelopeLookup(PsyGlobal _PsyGlobal, VorbisInfo _Info)
		{
			m_PsyGlobal = _PsyGlobal;
			var codecSetup = _Info.CodecSetup;

			m_MinEnergy  = codecSetup.PsyGlobalParam.PreEchoMinEnergy;
			m_Cursor     = codecSetup.BlockSizes[1] / 2;
			m_MdctWindow = new float[WindowLength];
			m_MdctLookup = new MdctLookup(WindowLength);

			for (var i = 0; i < m_MdctWindow.Length; i++)
			{
				m_MdctWindow[i] =  (float)Math.Sin(i / (WindowLength - 1.0) * Math.PI);
				m_MdctWindow[i] *= m_MdctWindow[i];
			}

			m_Bands = new EnvelopeBand[PsyGlobal.ENVELOPE_BANDS];

			// Magic follows
			m_Bands[0] = new EnvelopeBand(2, 4);
			m_Bands[1] = new EnvelopeBand(4, 5);
			m_Bands[2] = new EnvelopeBand(6, 6);
			m_Bands[3] = new EnvelopeBand(9, 8);
			m_Bands[4] = new EnvelopeBand(13, 8);
			m_Bands[5] = new EnvelopeBand(17, 8);
			m_Bands[6] = new EnvelopeBand(22, 8);

			m_Filters = Enumerable
				.Range(0, PsyGlobal.ENVELOPE_BANDS * _Info.Channels)
				.Select(_ => new EnvelopeFilterState())
				.ToArray();

			m_Mark = new int[WindowLength];
		}

		public void Shift(int _Shift)
		{
			var smallsize  = m_Current / SearchStep + ENVELOPE_POST;
			var smallshift = _Shift / SearchStep;

			Array.Copy(m_Mark, smallshift, m_Mark, 0, smallsize - smallshift);

			m_Current -= _Shift;
			if (m_CurrentMark >= 0)
				m_CurrentMark -= _Shift;
			m_Cursor -= _Shift;
		}

		public bool Mark(int _BeginWindow, int _EndWindow)
		{
			if (m_CurrentMark >= _BeginWindow && m_CurrentMark < _EndWindow)
				return true;

			var first = _BeginWindow / SearchStep;
			var last  = _EndWindow / SearchStep;

			for (var i = first; i < last; i++)
				if (m_Mark[i] != 0)
					return true;

			return false;
		}

		public int Search(
			float[][] _PCM,
			int       _PCMCurrent,
			int       _CenterWindow,
			int       _TestWindow
		)
		{
			var first = m_Current / SearchStep;
			var last  = _PCMCurrent / SearchStep - EnvelopeWindow;

			if (first < 0)
				first = 0;

			// make sure we have enough storage to match the PCM
			var requiredStorage = last + EnvelopeWindow + ENVELOPE_POST;
			if (requiredStorage > m_Mark.Length)
				m_Mark = new int[requiredStorage];

			for (var j = first; j < last; j++)
			{
				var ret = 0;

				m_Stretch++;
				if (m_Stretch > EnvelopeMaxStretch * 2)
					m_Stretch = EnvelopeMaxStretch * 2;

				for (var channel = 0; channel < _PCM.Length; channel++)
					ret |= AmpPcm(
						_PCM[channel],
						SearchStep * j,
						channel * PsyGlobal.ENVELOPE_BANDS
					);

				m_Mark[j + ENVELOPE_POST] = 0;
				if ((ret & 1) != 0)
				{
					m_Mark[j]     = 1;
					m_Mark[j + 1] = 1;
				}

				if ((ret & 2) != 0)
				{
					m_Mark[j] = 1;
					if (j > 0)
						m_Mark[j - 1] = 1;
				}

				if ((ret & 4) != 0)
					m_Stretch = -1;
			}

			m_Current = last * SearchStep;

			var l = m_Cursor;

			while (l < m_Current - SearchStep)
			{
				// account for postecho working back one window 
				if (l >= _TestWindow)
					return 1;

				m_Cursor = l;

				if (m_Mark[l / SearchStep] != 0)
					if (l > _CenterWindow)
					{
						m_CurrentMark = l;
						return l >= _TestWindow ? 1 : 0;
					}

				l += SearchStep;
			}

			return -1;
		}

		int AmpPcm(
			float[] _PCM,
			int     _PCMOffset,
			int     _FilterOffset
		)
		{
			var ret = 0;

			// we want to have a 'minimum bar' for energy, else we're just
			// basing blocks on quantization noise that outweighs the signal
			// itself (for low power signals) 
			Span<float> vec = stackalloc float[WindowLength];

			// stretch is used to gradually lengthen the number of windows considered previous-to-potential-trigger 
			var penalty = m_PsyGlobal.StretchPenalty - (m_Stretch / 2 - EnvelopeMinStretch);

			if (penalty < 0f)
				penalty = 0f;

			if (penalty > m_PsyGlobal.StretchPenalty)
				penalty = m_PsyGlobal.StretchPenalty;

			// window and transform 
			for (var i = 0; i < vec.Length; i++)
				vec[i] = _PCM[_PCMOffset + i] * m_MdctWindow[i];

			m_MdctLookup.Forward(vec, vec);

			// near-DC spreading function; ampPtr has nothing to do with
			// psychoacoustics, just sidelobe leakage and window size 
			var temp  = (float)(vec[0] * vec[0] + .7 * vec[1] * vec[1] + .2 * vec[2] * vec[2]);
			var decay = m_Filters[_FilterOffset].SpreadNearDc(temp);

			// perform spreading and limiting, also smooth the spectrum.  yes,
			// the MDCT results in all real coefficients, but it still *behaves*
			// like real/imaginary pairs 
			for (var i = 0; i < WindowLength / 2; i += 2)
			{
				var val = vec[i] * vec[i] + vec[i + 1] * vec[i + 1];
				val = val.ToDecibel() * .5f;

				if (val < decay)
					val = decay;

				if (val < m_MinEnergy)
					val = m_MinEnergy;

				vec[i >> 1] =  val;
				decay       -= 8;
			}

			// perform preecho/postecho triggering by band 
			for (var j = 0; j < m_Bands.Length; j++)
			{
				// accumulate amplitude 
				float acc = 0;
				for (var i = 0; i < m_Bands[j].Window.Length; i++)
					acc += vec[i + m_Bands[j].Begin] * m_Bands[j].Window[i];

				acc *= m_Bands[j].Total;

				// convert amplitude to delta 
				var stretch = Math.Max(EnvelopeMinStretch, m_Stretch / 2);
				var delta   = m_Filters[_FilterOffset + j].ConvertAmplitudeToDelta(acc, stretch);

				// look at min/max, decide trigger 
				if (delta.Max > m_PsyGlobal.PreEchoThreshold[j] + penalty)
				{
					ret |= 1;
					ret |= 4;
				}

				if (delta.Min < m_PsyGlobal.PostEchoThreshold[j] - penalty)
					ret |= 2;
			}

			return ret;
		}
	}
}
