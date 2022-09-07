using System;

namespace OggVorbisEncoder.Lookups
{
	public class EnvelopeFilterState
	{
		const    int     EnvelopePre = 16;
		const    int     EnvelopeAmp = EnvelopePre + EnvelopeLookup.ENVELOPE_POST - 1;
		readonly float[] m_AmpBuffer  = new float[EnvelopeAmp];

		readonly float[] m_NearBuffer = new float[15];
		int              m_AmpPointer;
		float            m_NearDcAcc;
		float            m_NearDcPartialAcc;

		int m_NearPointer;

		public float SpreadNearDc(float _Input)
		{
			// the accumulation is regularly refreshed from scratch to avoid floating point creep 
			if (m_NearPointer == 0)
			{
				m_NearDcAcc        = m_NearDcPartialAcc + _Input;
				m_NearDcPartialAcc = _Input;
			}
			else
			{
				m_NearDcAcc        += _Input;
				m_NearDcPartialAcc += _Input;
			}

			m_NearDcAcc                -= m_NearBuffer[m_NearPointer];
			m_NearBuffer[m_NearPointer] =  _Input;

			var decay = m_NearDcAcc;
			decay *= (float)(1.0 / (m_NearBuffer.Length + 1));

			m_NearPointer++;
			if (m_NearPointer >= m_NearBuffer.Length)
				m_NearPointer = 0;

			return (float)(decay.ToDecibel() * .5 - 15f);
		}

		public Delta ConvertAmplitudeToDelta(float _Amplitude, int _Stretch)
		{
			float preMax = -99999f, preMin = 99999f;

			var p = m_AmpPointer;
			if (--p < 0)
				p += EnvelopeAmp;

			var postMax = Math.Max(_Amplitude, m_AmpBuffer[p]);
			var postMin = Math.Min(_Amplitude, m_AmpBuffer[p]);

			for (var i = 0; i < _Stretch; i++)
			{
				if (--p < 0)
					p += EnvelopeAmp;

				preMax = Math.Max(preMax, m_AmpBuffer[p]);
				preMin = Math.Min(preMin, m_AmpBuffer[p]);
			}

			m_AmpBuffer[m_AmpPointer] = _Amplitude;
			m_AmpPointer++;

			if (m_AmpPointer >= m_AmpBuffer.Length)
				m_AmpPointer = 0;

			return new Delta(
				postMin - preMin,
				postMax - preMax
			);
		}
	}
}