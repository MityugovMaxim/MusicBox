using UnityEngine;

public class VolumeSampler : MonoBehaviour
{
	[SerializeField] AudioSource  m_AudioSource;
	[SerializeField] UISpectrum[] m_Items;
	[SerializeField] int          m_Samples  = 64;
	[SerializeField] int          m_Channels = 1;

	int     m_SamplesCache;
	int     m_ChannelsCache;
	float[] m_Spectrum;
	float[] m_Amplitude;

	void Awake()
	{
		m_SamplesCache  = Mathf.Max(64, Mathf.IsPowerOfTwo(m_Samples) ? m_Samples : Mathf.ClosestPowerOfTwo(m_Samples));
		m_ChannelsCache = Mathf.Max(1, m_Channels);
		
		m_Spectrum  = new float[m_SamplesCache];
		m_Amplitude = new float[m_ChannelsCache];
	}

	void Update()
	{
		for (int channel = 0; channel < m_ChannelsCache; channel++)
			Process(channel);
		
		foreach (UISpectrum item in m_Items)
			item.Sample(m_Amplitude);
	}

	void Process(int _Channel)
	{
		m_AudioSource.GetOutputData(m_Spectrum, _Channel);
		
		float rms = 0;
		foreach (float spectrum in m_Spectrum)
			rms += spectrum * spectrum;
		rms = Mathf.Sqrt(rms / m_Spectrum.Length);
		m_Amplitude[_Channel] = rms;
	}
}