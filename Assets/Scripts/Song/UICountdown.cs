using System;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UICountdown : UIOrder
{
	static readonly int m_PlayParameterID = Animator.StringToHash("Play");

	[Inject] SoundProcessor m_SoundProcessor;

	Animator m_Animator;

	Action m_Finished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.RegisterComplete("play", InvokePlayFinished);
	}

	public void Play()
	{
		if (m_Animator != null)
			m_Animator.SetTrigger(m_PlayParameterID);
	}

	[Preserve]
	void PlaySound(string _SoundID)
	{
		m_SoundProcessor.Play(_SoundID);
	}

	void InvokePlayFinished()
	{
		Action action = m_Finished;
		m_Finished = null;
		action?.Invoke();
	}
}