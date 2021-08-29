using UnityEngine;

public class MusicClip : Clip
{
	[SerializeField] AudioClip m_AudioClip;

	AudioSource m_AudioSource;

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
		
		m_AudioSource.clip = m_AudioClip;
		m_AudioSource.time = GetMusicTime(_Time);
		m_AudioSource.PlayScheduled(AudioSettings.dspTime + MusicTrack.DSP_TIME_OFFSET);
	}

	protected override void OnUpdate(float _Time)
	{
		if (!Sequencer.Playing && Playing)
		{
			m_AudioSource.Pause();
			
			m_AudioSource.time = GetMusicTime(_Time);
		}
		else if (Sequencer.Playing && Playing && !m_AudioSource.isPlaying && _Time < MaxTime)
		{
			AudioManager.SetAudioActive(true);
			
			m_AudioSource.clip = m_AudioClip;
			m_AudioSource.time = GetMusicTime(_Time);
			m_AudioSource.PlayScheduled(AudioSettings.dspTime + MusicTrack.DSP_TIME_OFFSET);
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