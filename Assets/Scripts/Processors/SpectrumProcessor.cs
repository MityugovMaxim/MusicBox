using System;
using System.Linq;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(AudioSource))]
public class SpectrumProcessor : MonoBehaviour
{
	static readonly int m_SpectrumPropertyID = Shader.PropertyToID("_Spectrum");

	[SerializeField] float m_DampenStep = 0.005f;

	SignalBus      m_SignalBus;
	LevelProcessor m_LevelProcessor;
	AudioSource    m_AudioSource;

	readonly float[] m_Buffer          = new float[64];
	readonly float[] m_Spectrum        = new float[64];
	readonly float[] m_SpectrumDampen  = new float[32];

	float[] m_Amplitude;

	public void SetAmplitude(float[] _Amplitude)
	{
		if (_Amplitude != null && _Amplitude.Length == 32)
			m_Amplitude = _Amplitude;
	}

	void Awake()
	{
		m_AudioSource = GetComponent<AudioSource>();
	}

	void Update()
	{
		m_AudioSource.GetSpectrumData(m_Buffer, 0, FFTWindow.BlackmanHarris);
		
		for (int i = 0; i < 32; i++)
			m_Spectrum[i] = Mathf.Max(0, m_Spectrum[i] - m_SpectrumDampen[i]);
		
		int index = 0;
		for (var i = 0; i < 32; i++)
		{
			float spectrum = 0;
			spectrum += m_Buffer[index++];
			spectrum += m_Buffer[index++];
			spectrum *= 0.5f;
			
			if (m_Amplitude[i] < spectrum)
				m_Amplitude[i] = spectrum;
			
			if (spectrum > float.Epsilon && m_Amplitude[i] > float.Epsilon)
				spectrum /= m_Amplitude[i];
			
			if (m_Spectrum[i] < spectrum)
			{
				m_Spectrum[i]       = spectrum;
				m_SpectrumDampen[i] = 0;
			}
			
			m_SpectrumDampen[i] += m_DampenStep;
		}
		
		for (int i = 0; i < 32; i++)
			m_Spectrum[32 + i] = m_Spectrum[i];
		
		Shader.SetGlobalFloatArray(m_SpectrumPropertyID, m_Spectrum);
	}
}