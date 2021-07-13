using JetBrains.Annotations;
using UnityEngine;

public class MusicClip : Clip
{
	[SerializeField] AudioClip m_AudioClip;

	[SerializeField, UsedImplicitly] float m_MinOffset;
	[SerializeField, UsedImplicitly] float m_MaxOffset;

	AudioSource m_AudioSource;

	public void Initialize(Sequencer _Sequencer, AudioSource _AudioSource)
	{
		base.Initialize(_Sequencer);
		
		m_AudioSource = _AudioSource;
	}

	protected override void OnEnter(float _Time)
	{
		if (!Sequencer.Playing || !Playing)
			return;
		
		float time = Mathf.Clamp(GetLocalTime(_Time) + m_MinOffset, 0, m_AudioClip.length);
		
		m_AudioSource.clip = m_AudioClip;
		
		m_AudioSource.Play();
		
		m_AudioSource.time = time;
	}

	protected override void OnUpdate(float _Time)
	{
		float time = Mathf.Clamp(GetLocalTime(_Time) + m_MinOffset, 0, m_AudioClip.length);
		
		if (!Sequencer.Playing && Playing)
		{
			m_AudioSource.Pause();
			
			m_AudioSource.time = time;
		}
		else if (Sequencer.Playing && Playing && !m_AudioSource.isPlaying && _Time < MaxTime)
		{
			m_AudioSource.clip = m_AudioClip;
			
			m_AudioSource.Play();
			
			m_AudioSource.time = time;
		}
	}

	protected override void OnExit(float _Time)
	{
		m_AudioSource.Stop();
	}
}