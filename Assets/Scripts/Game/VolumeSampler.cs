using UnityEngine;

public class VolumeSampler : MonoBehaviour
{
	[SerializeField]              AudioSource  m_AudioSource;
	[SerializeField]              UISpectrum[] m_Items;
	[SerializeField]              int          m_Samples    = 64;
	[SerializeField]              int          m_Channels   = 1;
	[SerializeField, Range(0, 1)] float        m_AttackDamp = 0.3f;
	[SerializeField, Range(0, 1)] float        m_DecayDamp  = 0.15f;

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
		
		float source = m_Amplitude[_Channel];
		float target = 0;
		foreach (float spectrum in m_Spectrum)
			target += spectrum * spectrum;
		target = Mathf.Sqrt(target / m_Spectrum.Length);
		
		m_Amplitude[_Channel] = target > source
			? Mathf.Lerp(source, target, m_AttackDamp)
			: Mathf.Lerp(source, target, m_DecayDamp);
	}
}