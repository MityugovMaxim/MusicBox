using UnityEngine;

public class Spectrum : MonoBehaviour
{
	static readonly float[] m_Buffer   = new float[64];
	static readonly float[] m_Spectrum = new float[64];
	static readonly float[] m_Falloff  = new float[32];

	static readonly int m_SpectrumPropertyID = Shader.PropertyToID("_Spectrum");

	[SerializeField] AudioSource m_AudioSource;
	[SerializeField] float       m_Dampen = 1.25f;
	[SerializeField] FFTWindow   m_FFT    = FFTWindow.BlackmanHarris;

	void Awake()
	{
		for (int i = 0; i < 32; i++)
			m_Falloff[i] = Mathf.Sqrt(Mathf.Log(Mathf.Max(2, i + 1)));
	}

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
		
		for (int i = 0; i < 32; i++)
		{
			float spectrum = Mathf.Sqrt(m_Buffer[i]) * m_Falloff[i];
			int   index    = 32 * _Channel + i;
			
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