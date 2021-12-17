using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UIReward : UIEntity
{
	static readonly int m_RestoreParameter = Animator.StringToHash("Restore");
	static readonly int m_PlayParameter    = Animator.StringToHash("Play");

	[SerializeField] UIPath m_Path;

	Animator m_Animator;

	StateBehaviour m_PlayState;

	Action m_PlayFinished;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
		
		m_PlayState = StateBehaviour.GetBehaviour(m_Animator, "play");
		if (m_PlayState != null)
			m_PlayState.OnComplete += InvokePlayFinished;
	}

	public void Setup(UIEntity _Entity)
	{
		Restore();
		
		if (m_Path != null && _Entity != null)
			m_Path.Target = _Entity.RectTransform;
	}

	public Task Play()
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		Play(() => completionSource.SetResult(true));
		
		return completionSource.Task;
	}

	public void Play(Action _Finished)
	{
		if (!gameObject.activeInHierarchy)
		{
			_Finished?.Invoke();
			return;
		}
		
		InvokePlayFinished();
		
		m_PlayFinished = _Finished;
		
		m_Animator.SetTrigger(m_PlayParameter);
	}

	public void Restore()
	{
		if (m_Animator == null)
			return;
		
		m_Animator.ResetTrigger(m_PlayParameter);
		
		m_Animator.SetTrigger(m_RestoreParameter);
	}

	void InvokePlayFinished()
	{
		Action action = m_PlayFinished;
		m_PlayFinished = null;
		action?.Invoke();
	}
}