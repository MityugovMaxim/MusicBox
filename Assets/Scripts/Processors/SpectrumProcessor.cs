using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpectrumProcessor : MonoBehaviour
{
	static readonly int m_SpectrumPropertyID = Shader.PropertyToID("_Spectrum");

	[SerializeField] float m_SpectrumDampenStep;
	[SerializeField] float m_AmplitudeDampenStep;

	AudioSource m_AudioSource;

	readonly float[] m_Buffer          = new float[64];
	readonly float[] m_Amplitude       = new float[64];
	readonly float[] m_Spectrum        = new float[64];
	readonly float[] m_SpectrumDampen  = new float[64];
	readonly float[] m_AmplitudeDampen = new float[64];

	void Awake()
	{
		m_AudioSource = GetComponent<AudioSource>();
		
		for (int i = 0; i < m_Amplitude.Length; i++)
			m_Amplitude[i] = float.Epsilon * 2;
	}

	void Update()
	{
		m_AudioSource.GetSpectrumData(m_Buffer, 0, FFTWindow.BlackmanHarris);
		
		for (int i = 0; i < m_Spectrum.Length; i++)
			m_Spectrum[i] = Mathf.Max(0, m_Spectrum[i] - m_SpectrumDampen[i]);
		
		// for (int i = 0; i < m_Amplitude.Length; i++)
		// 	m_Amplitude[i] = Mathf.Max(0.001f, m_Amplitude[i] - m_AmplitudeDampen[i]);
		
		int index = 0;
		for (var i = 0; i < m_Buffer.Length; i++)
		{
			int size = 1;
			
			float spectrum = 0;
			for (int j = 0; j < size; j++)
				spectrum += m_Buffer[index++];
			spectrum /= size;
			
			if (m_Amplitude[i] < spectrum)
			{
				m_Amplitude[i]       = spectrum;
				m_AmplitudeDampen[i] = 0;
			}
			
			if (spectrum > float.Epsilon && m_Amplitude[i] > float.Epsilon)
				spectrum /= m_Amplitude[i];
			
			if (m_Spectrum[i] < spectrum)
			{
				m_Spectrum[i]       = spectrum;
				m_SpectrumDampen[i] = 0;
			}
			
			m_SpectrumDampen[i]  += m_SpectrumDampenStep;
			m_AmplitudeDampen[i] += m_AmplitudeDampenStep;
		}
		
		Shader.SetGlobalFloatArray(m_SpectrumPropertyID, m_Spectrum);
	}
}