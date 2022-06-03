using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UIAnimationMenu : UIMenu
{
	static readonly int m_ShowParameterID    = Animator.StringToHash("Show");
	static readonly int m_InstantParameterID = Animator.StringToHash("Instant");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	Animator m_Animator;

	Action m_ShowFinished;
	Action m_HideFinished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_Animator.RegisterComplete("show", InvokeShowFinished);
		m_Animator.RegisterComplete("hide", InvokeHideFinished);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		InvokeShowFinished();
		InvokeHideFinished();
		
		m_Animator.UnregisterComplete("show", InvokeShowFinished);
		m_Animator.UnregisterComplete("hide", InvokeHideFinished);
	}

	protected override async Task ShowAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		Restore();
		
		await AlphaAnimation(1, _Duration, true, _Token);
		
		await Show(_Instant, _Token);
	}

	protected override async Task HideAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		await Hide(_Instant, _Token);
		
		await AlphaAnimation(0, _Duration, true, _Token);
	}

	void Restore()
	{
		InvokeShowFinished();
		InvokeHideFinished();
		
		if (m_Animator == null)
			return;
		
		m_Animator.SetBool(m_ShowParameterID, false);
		m_Animator.ResetTrigger(m_InstantParameterID);
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	Task Show(bool _Instant = false, CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return Task.CompletedTask;
		
		InvokeShowFinished();
		InvokeHideFinished();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		_Token.Register(() => completionSource.TrySetResult(false));
		
		m_ShowFinished = () => completionSource.TrySetResult(true);
		
		if (m_Animator != null)
		{
			m_Animator.SetBool(m_ShowParameterID, true);
			m_Animator.SetBool(m_InstantParameterID, _Instant);
		}
		else
		{
			InvokeShowFinished();
		}
		
		return completionSource.Task;
	}

	Task Hide(bool _Instant = false, CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return Task.CompletedTask;
		
		InvokeShowFinished();
		InvokeHideFinished();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		_Token.Register(() => completionSource.TrySetResult(false));
		
		m_HideFinished = () => completionSource.TrySetResult(true);
		
		if (m_Animator != null)
		{
			m_Animator.SetBool(m_ShowParameterID, false);
			m_Animator.SetBool(m_InstantParameterID, _Instant);
		}
		else
		{
			InvokeHideFinished();
		}
		
		return completionSource.Task;
	}

	void InvokeShowFinished()
	{
		Action action = m_ShowFinished;
		m_ShowFinished = null;
		action?.Invoke();
	}

	void InvokeHideFinished()
	{
		Action action = m_HideFinished;
		m_HideFinished = null;
		action?.Invoke();
	}
}