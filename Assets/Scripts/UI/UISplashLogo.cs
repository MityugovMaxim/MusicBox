using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UISplashLogo : UIEntity
{
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");

	[SerializeField, Sound] string m_Sound;

	[Inject] SoundProcessor m_SoundProcessor;

	Animator m_Animator;
	Action   m_PlayFinished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
		
		m_Animator.SubscribeComplete("play", InvokePlayFinished);
	}

	public Task PlayAsync()
	{
		Restore();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_PlayFinished = () => completionSource.TrySetResult(true);
		
		m_SoundProcessor.Play(m_Sound);
		
		m_Animator.SetTrigger(m_PlayParameterID);
		
		return completionSource.Task;
	}

	void Restore()
	{
		InvokePlayFinished();
		
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	void InvokePlayFinished()
	{
		Action action = m_PlayFinished;
		m_PlayFinished = null;
		action?.Invoke();
	}
}