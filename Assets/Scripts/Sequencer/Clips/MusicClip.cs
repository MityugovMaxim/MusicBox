using UnityEngine;

public class MusicClip : Clip
{
	#if UNITY_EDITOR
	const float DSP_TIME_OFFSET = 0;
	#else
	const float DSP_TIME_OFFSET = 1;
	#endif

	public override float MinTime
	{
		get => base.MinTime - DSP_TIME_OFFSET;
		set => base.MinTime = value;
	}

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
		
		m_AudioSource.clip = m_AudioClip;
		#if UNITY_EDITOR
		m_AudioSource.Play();
		#else
		m_AudioSource.PlayScheduled(AudioSettings.dspTime + DSP_TIME_OFFSET);
		#endif
		AudioManager.SetAudioActive(true);
		
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
			m_AudioSource.clip = m_AudioClip;
			#if UNITY_EDITOR
			m_AudioSource.Play();
			#else
			m_AudioSource.PlayScheduled(AudioSettings.dspTime + DSP_TIME_OFFSET);
			#endif
			AudioManager.SetAudioActive(true);
			
			m_AudioSource.time = GetMusicTime(_Time);
		}
	}

	protected override void OnExit(float _Time)
	{
		m_AudioSource.Stop();
	}

	float GetMusicTime(float _Time)
	{
		float time = _Time - (MinTime + DSP_TIME_OFFSET);
		
		return Mathf.Clamp(time, 0, m_AudioClip.length);
	}
}