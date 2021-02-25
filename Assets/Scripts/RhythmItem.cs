using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RhythmItem : MonoBehaviour
{
	Animator Animator
	{
		get
		{
			if (m_Animator == null)
				m_Animator = GetComponent<Animator>();
			return m_Animator;
		}
	}

	static readonly int m_SpeedParameterID   = Animator.StringToHash("Speed");
	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_SuccessParameterID = Animator.StringToHash("Success");
	static readonly int m_FailParameterID    = Animator.StringToHash("Fail");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	Animator       m_Animator;
	StateBehaviour m_PlayState;
	StateBehaviour m_SuccessState;
	StateBehaviour m_FailState;
	Action         m_PlayFinished;
	Action         m_SuccessFinished;
	Action         m_FailFinished;

	void Awake()
	{
		m_PlayState = StateBehaviour.GetBehaviour(Animator, "play");
		if (m_PlayState != null)
			m_PlayState.OnComplete += InvokePlayFinished;
		
		m_SuccessState = StateBehaviour.GetBehaviour(Animator, "success");
		if (m_SuccessState != null)
			m_SuccessState.OnComplete += InvokeSuccessFinished;
		
		m_FailState = StateBehaviour.GetBehaviour(Animator, "fail");
		if (m_FailState != null)
			m_FailState.OnComplete += InvokeFailFinished;
	}

	void OnDestroy()
	{
		if (m_PlayState != null)
			m_PlayState.OnComplete -= InvokePlayFinished;
		
		if (m_SuccessState != null)
			m_SuccessState.OnComplete -= InvokeSuccessFinished;
		
		if (m_FailState != null)
			m_FailState.OnComplete -= InvokeFailFinished;
	}

	public void Play(float _Duration, Action _Finished = null)
	{
		InvokePlayFinished();
		
		m_PlayFinished = _Finished;
		
		float speed = 1.0f / _Duration;
		
		Animator.SetFloat(m_SpeedParameterID, speed);
		Animator.SetTrigger(m_PlayParameterID);
	}

	public void Success(Action _Finished = null)
	{
		InvokeSuccessFinished();
		
		m_SuccessFinished = _Finished;
		
		Animator.SetTrigger(m_SuccessParameterID);
	}

	public void Fail(Action _Finished = null)
	{
		InvokeFailFinished();
		
		m_FailFinished = _Finished;
		
		Animator.SetTrigger(m_FailParameterID);
	}

	public void Remove()
	{
		Animator.SetFloat(m_SpeedParameterID, 1);
		Animator.ResetTrigger(m_PlayParameterID);
		Animator.SetTrigger(m_RestoreParameterID);
		
		Destroy(gameObject);
	}

	void InvokePlayFinished()
	{
		Action action = m_PlayFinished;
		m_PlayFinished = null;
		action?.Invoke();
	}

	void InvokeSuccessFinished()
	{
		Action action = m_SuccessFinished;
		m_SuccessFinished = null;
		action?.Invoke();
	}

	void InvokeFailFinished()
	{
		Action action = m_FailFinished;
		m_FailFinished = null;
		action?.Invoke();
	}
}