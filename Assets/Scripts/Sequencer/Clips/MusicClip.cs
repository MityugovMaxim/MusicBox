using UnityEngine;

public class MusicClip : Clip
{
	const float DSP_TIME_OFFSET = 1;

	[SerializeField] AudioClip m_AudioClip;

	AudioSource m_AudioSource;
	bool        m_Paused;
	float       m_Latency;

	public override float MinOffset => -DSP_TIME_OFFSET;

	public void Initialize(Sequencer _Sequencer, AudioSource _AudioSource)
	{
		base.Initialize(_Sequencer);
		
		m_AudioSource = _AudioSource;
		
		m_AudioClip.LoadAudioData();
	}

	protected override void OnEnter(float _Time)
	{
		if (!Sequencer.Playing || !Playing)
			return;
		
		m_Paused  = false;
		m_Latency = AudioManager.Latency;
		
		if (m_AudioSource.clip != m_AudioClip)
			m_AudioSource.clip = m_AudioClip;
		
		if (MinTime > _Time)
		{
			double delta = (double)MinTime - _Time;
			
			m_AudioSource.PlayScheduled(AudioSettings.dspTime + delta - m_Latency);
		}
		else
		{
			m_AudioSource.Play();
		}
		
		m_AudioSource.time = GetMusicTime(_Time);
	}

	protected override void OnUpdate(float _Time)
	{
		if (!Sequencer.Playing && !m_Paused)
		{
			m_Paused = true;
			m_AudioSource.Pause();
		}
		
		if (Sequencer.Playing && m_Paused)
		{
			m_Paused = false;
			m_AudioSource.UnPause();
		}
		
		if (!Mathf.Approximately(m_Latency, AudioManager.Latency))
		{
			m_AudioSource.time += AudioManager.Latency - m_Latency;
			m_Latency          =  AudioManager.Latency;
		}
	}

	protected override void OnExit(float _Time)
	{
		m_Paused = false;
		
		m_AudioSource.Stop();
	}

	float GetMusicTime(float _Time)
	{
		return Mathf.Clamp(GetLocalTime(_Time), 0, m_AudioClip.length);
	}
}