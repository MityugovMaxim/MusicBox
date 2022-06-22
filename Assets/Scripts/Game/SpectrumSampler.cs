using System;
using UnityEngine;

public class SpectrumSampler : MonoBehaviour
{
	[SerializeField]              AudioSource  m_AudioSource;
	[SerializeField]              UISpectrum[] m_Items;
	[SerializeField]              FFTWindow    m_FFTWindow    = FFTWindow.BlackmanHarris;
	[SerializeField]              int          m_Samples      = 128;
	[SerializeField]              int          m_Bends        = 32;
	[SerializeField]              int          m_Channels     = 2;
	[SerializeField]              float        m_MinFrequency = 0;
	[SerializeField]              float        m_MaxFrequency = 22050;
	[SerializeField]              float        m_MaxAmplitude = 1;
	[SerializeField]              float        m_MinAmplitude = 0.1f;
	[SerializeField, Range(0, 1)] float        m_AttackDamp   = 0.3f;
	[SerializeField, Range(0, 1)] float        m_DecayDamp    = 0.15f;

	[NonSerialized] int     m_SamplesCache;
	[NonSerialized] int     m_ChannelsCache;
	[NonSerialized] int     m_BendsCache;
	[NonSerialized] float[] m_Spectrum;
	[NonSerialized] float[] m_Amplitude;

	[NonSerialized] float m_FrequencyLogFactor;
	[NonSerialized] float m_FrequencyScaleFactor;

	void Awake()
	{
		int frequency = AudioSettings.outputSampleRate / 2;
		
		m_SamplesCache  = Mathf.Max(64, Mathf.ClosestPowerOfTwo(m_Samples));
		m_ChannelsCache = Mathf.Max(1, m_Channels);
		m_BendsCache    = Mathf.CeilToInt((float)m_Bends / m_ChannelsCache);
		
		m_Spectrum  = new float[m_SamplesCache];
		m_Amplitude = new float[m_BendsCache * m_ChannelsCache];
		
		m_FrequencyLogFactor   = Mathf.Log(m_BendsCache + 1, 2);
		m_FrequencyScaleFactor = 1.0f / frequency * m_SamplesCache;
	}

	void Update()
	{
		ProcessAmplitude(0);
		
		for (int channel = 0; channel < m_ChannelsCache; channel++)
			ProcessAmplitude(channel);
		
		foreach (UISpectrum item in m_Items)
			item.Sample(m_Amplitude);
	}

	void ProcessAmplitude(int _Channel)
	{
		m_AudioSource.GetSpectrumData(m_Spectrum, _Channel, m_FFTWindow);
		
		for (int i = 0; i < m_BendsCache; i++)
		{
			float phase = Mathf.Lerp(
				m_MinFrequency,
				m_MaxFrequency,
				(m_FrequencyLogFactor - Mathf.Log(m_BendsCache + 1 - i, 2)) / m_FrequencyLogFactor
			);
			phase *= m_FrequencyScaleFactor;
			
			int index = Mathf.Clamp(Mathf.FloorToInt(phase), 0, m_Spectrum.Length - 2);
			
			float value = Mathf.SmoothStep(
				m_Spectrum[index],
				m_Spectrum[index + 1],
				phase - index
			);
			
			value = Mathf.Sqrt(value * (phase + 1));
			
			int address = m_BendsCache * _Channel + i;
			
			float amplitude = m_Amplitude[address];
			
			float damp = value * m_MaxAmplitude > amplitude
				? m_AttackDamp
				: m_DecayDamp;
			
			m_Amplitude[address] = Mathf.Lerp(amplitude, Mathf.Max(value * m_MaxAmplitude, m_MinAmplitude), damp);
		}
	}
}
