using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpectrumProcessor : MonoBehaviour
{
	static readonly int m_SpectrumPropertyID = Shader.PropertyToID("_Spectrum");

	static readonly int[] m_Sizes = { 1, 2, 4, 8, 16, 33 };

	[SerializeField] float m_DampenStep;

	AudioSource m_AudioSource;

	readonly float[] m_Buffer    = new float[64];
	readonly float[] m_Amplitude = new float[64];
	readonly float[] m_Dampen    = new float[64];
	readonly float[] m_Spectrum  = new float[64];

	void Awake()
	{
		m_AudioSource = GetComponent<AudioSource>();
	}

	void Update()
	{
		m_AudioSource.GetSpectrumData(m_Buffer, 0, FFTWindow.BlackmanHarris);
		
		for (int i = 0; i < m_Spectrum.Length; i++)
			m_Spectrum[i] = Mathf.Max(0, m_Spectrum[i] - m_Dampen[i]);
		
		int index = 0;
		for (var i = 0; i < m_Buffer.Length; i++)
		{
			int size = 1;
			
			float spectrum = 0;
			for (int j = 0; j < size; j++)
				spectrum += m_Buffer[index++];
			spectrum /= size;
			
			if (m_Amplitude[i] < spectrum)
				m_Amplitude[i] = spectrum;
			
			if (spectrum > float.Epsilon && m_Amplitude[i] > float.Epsilon)
				spectrum /= m_Amplitude[i];
			
			if (m_Spectrum[i] < spectrum)
			{
				m_Spectrum[i] = spectrum;
				m_Dampen[i]   = 0;
			}
			
			m_Dampen[i] += m_DampenStep;
		}
		
		Shader.SetGlobalFloatArray(m_SpectrumPropertyID, m_Spectrum);
	}
}