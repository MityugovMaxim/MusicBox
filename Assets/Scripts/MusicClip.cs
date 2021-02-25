using System;
using UnityEngine;

[Serializable]
public class MusicClip : Clip
{
	[SerializeField] AudioClip m_AudioClip;

	AudioSource m_AudioSource;

	public void Initialize(AudioSource _AudioSource)
	{
		m_AudioSource = _AudioSource;
	}

	protected override void OnEnter(float _Time)
	{
		float time = GetLocalTime(_Time);
		
		m_AudioSource.time = time;
		m_AudioSource.clip = m_AudioClip;
		
		m_AudioSource.Play();
	}

	protected override void OnUpdate(float _Time) { }

	protected override void OnExit(float _Time)
	{
		m_AudioSource.Stop();
	}
}