using UnityEngine;

public class MusicClip : Clip
{
	public override float MinTime
	{
		get => base.MinTime - AudioManager.Latency;
		set => base.MinTime = value;
	}

	public override float MaxTime
	{
		get => base.MaxTime - AudioManager.Latency;
		set => base.MaxTime = value;
	}

	[SerializeField] AudioClip m_AudioClip;

	AudioSource m_AudioSource;

	public void Initialize(Sequencer _Sequencer, AudioSource _AudioSource)
	{
		base.Initialize(_Sequencer);
		
		AudioManager.SetAudioActive(true);
		
		m_AudioSource      = _AudioSource;
		m_AudioSource.clip = m_AudioClip;
		
		m_AudioClip.LoadAudioData();
	}

	protected override void OnEnter(float _Time)
	{
		if (!Sequencer.Playing || !Playing)
			return;
		
		AudioManager.SetAudioActive(true);
		m_AudioSource.Play();
		
		m_AudioSource.time = GetMusicTime(_Time);
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
			m_AudioSource.Play();
			
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