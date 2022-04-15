using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(Animator))]
public class UITapFX : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UITapFX> { }

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");

	Animator m_Animator;
	Action   m_PlayFinished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
		
		m_Animator.RegisterComplete("play", InvokePlayFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Animator.UnregisterComplete("play", InvokePlayFinished);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		Restore();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Restore();
	}

	public Task PlayAsync()
	{
		Restore();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_PlayFinished = () => completionSource.TrySetResult(true);
		
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