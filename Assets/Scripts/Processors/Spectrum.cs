using UnityEngine;

public class Spectrum : MonoBehaviour
{
	const int SAMPLES = 64;

	static readonly float[] m_Buffer   = new float[SAMPLES];
	static readonly float[] m_Spectrum = new float[SAMPLES];

	static readonly int m_SpectrumPropertyID = Shader.PropertyToID("_Spectrum");

	[SerializeField] AudioSource m_AudioSource;
	[SerializeField] float       m_Dampen = 1.25f;
	[SerializeField] FFTWindow   m_FFT    = FFTWindow.BlackmanHarris;

	void Update()
	{
		// Left channel
		Process(0);
		
		// Right channel
		Process(1);
		
		Shader.SetGlobalFloatArray(m_SpectrumPropertyID, m_Spectrum);
		
		Dampen();
	}

	void Process(int _Channel)
	{
		m_AudioSource.GetSpectrumData(m_Buffer, _Channel, m_FFT);
		
		for (int i = 0; i < SAMPLES / 2; i++)
		{
			float spectrum = Mathf.Sqrt(m_Buffer[i]) * Mathf.Sqrt(Mathf.Log(i + 1));
			int   index    = SAMPLES / 2 * _Channel + i;
			
			m_Spectrum[index] = Mathf.Max(m_Spectrum[index], spectrum);
		}
	}

	void Dampen()
	{
		for (int i = 0; i < m_Spectrum.Length; i++)
		{
			float dampen = Mathf.Max(0.01f, m_Dampen) * Time.deltaTime;
			m_Spectrum[i] = Mathf.Max(0, m_Spectrum[i] - dampen);
		}
	}
}