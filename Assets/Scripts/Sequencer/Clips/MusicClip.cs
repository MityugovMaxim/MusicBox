using UnityEngine;

public class MusicClip : Clip
{
	const float DSP_TIME_OFFSET = 1;

	[SerializeField] AudioClip m_AudioClip;

	AudioSource m_AudioSource;

	public override float MinOffset => -DSP_TIME_OFFSET;

	public void Initialize(Sequencer _Sequencer, AudioSource _AudioSource)
	{
		base.Initialize(_Sequencer);
		
		m_AudioSource = _AudioSource;
		
		m_AudioClip.LoadAudioData();
		
		AudioManager.SetAudioActive(true);
	}

	protected override void OnEnter(float _Time)
	{
		if (!Sequencer.Playing || !Playing)
			return;
		
		AudioManager.SetAudioActive(true);
		
		if (m_AudioSource.clip != m_AudioClip)
			m_AudioSource.clip = m_AudioClip;
		
		if (_Time < MinTime)
			m_AudioSource.PlayScheduled(AudioSettings.dspTime + MinTime - _Time);
		else
			m_AudioSource.Play();
		
		m_AudioSource.time = GetMusicTime(_Time);
	}

	protected override void OnUpdate(float _Time)
	{
		if (!Sequencer.Playing)
		{
			m_AudioSource.Pause();
		}
		else if (Sequencer.Playing && !m_AudioSource.isPlaying && _Time >= MinTime && _Time < MaxTime)
		{
			AudioManager.SetAudioActive(true);
			
			if (m_AudioSource.clip != m_AudioClip)
				m_AudioSource.clip = m_AudioClip;
			
			m_AudioSource.UnPause();
			m_AudioSource.time = GetMusicTime(_Time);
		}
	}

	protected override void OnExit(float _Time)
	{
		m_AudioSource.Stop();
	}

	float GetMusicTime(float _Time)
	{
		return Mathf.Clamp(GetLocalTime(_Time), 0, m_AudioClip.length);
	}
}