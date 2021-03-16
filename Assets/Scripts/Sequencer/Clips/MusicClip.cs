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
		float time = Mathf.Clamp(GetLocalTime(_Time) + m_MinOffset, 0, m_AudioClip.length);
		
		#if UNITY_EDITOR
		if (!Application.isPlaying && Sequencer.Playing && Playing)
		{
			AudioUtility.PlayClip(m_AudioClip);
			AudioUtility.SetClipSamplePosition(m_AudioClip, time);
			return;
		}
		#endif
		
		if (Sequencer.Playing && Playing)
		{
			m_AudioSource.clip = m_AudioClip;
			
			m_AudioSource.Play();
			
			m_AudioSource.time = time;
		}
	}

	protected override void OnUpdate(float _Time)
	{
		float time = Mathf.Clamp(GetLocalTime(_Time) + m_MinOffset, 0, m_AudioClip.length);
		
		#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			if (!Sequencer.Playing && Playing)
			{
				AudioUtility.StopClip(m_AudioClip);
				AudioUtility.SetClipSamplePosition(m_AudioClip, time);
			}
			else if (Sequencer.Playing && Playing && !AudioUtility.IsClipPlaying(m_AudioClip))
			{
				AudioUtility.PlayClip(m_AudioClip);
				AudioUtility.SetClipSamplePosition(m_AudioClip, time);
			}
			return;
		}
		#endif
		
		m_AudioSource.clip = m_AudioClip;
		
		if (!Sequencer.Playing && Playing)
		{
			m_AudioSource.Pause();
			
			m_AudioSource.time = time;
		}
		else if (Sequencer.Playing && Playing && !m_AudioSource.isPlaying)
		{
			m_AudioSource.Play();
			
			m_AudioSource.time = time;
		}
	}

	protected override void OnExit(float _Time)
	{
		#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			AudioUtility.StopClip(m_AudioClip);
			return;
		}
		#endif
		
		m_AudioSource.Stop();
	}
}