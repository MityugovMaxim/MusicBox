using JetBrains.Annotations;
using UnityEngine;

public class MusicClip : Clip
{
	[SerializeField] AudioClip m_AudioClip;

	[SerializeField, UsedImplicitly] float m_MinOffset;
	[SerializeField, UsedImplicitly] float m_MaxOffset;

	AudioSource m_AudioSource;

	public void Initialize(AudioSource _AudioSource)
	{
		m_AudioSource = _AudioSource;
	}

	protected override void OnEnter(float _Time)
	{
		float time = GetLocalTime(_Time);
		
		if (Application.isPlaying)
		{
			m_AudioSource.clip = m_AudioClip;
			
			m_AudioSource.Play();
			
			m_AudioSource.time = time + m_MinOffset;
		}
		else
		{
			AudioUtility.PlayClip(m_AudioClip);
			AudioUtility.SetClipSamplePosition(m_AudioClip, time + m_MinOffset);
		}
	}

	protected override void OnUpdate(float _Time) { }

	protected override void OnExit(float _Time)
	{
		if (Application.isPlaying)
			m_AudioSource.Stop();
		else
			AudioUtility.StopClip(m_AudioClip);
	}

	protected override void OnStop(float _Time)
	{
		if (Application.isPlaying)
			m_AudioSource.Stop();
		else
			AudioUtility.StopClip(m_AudioClip);
	}
}