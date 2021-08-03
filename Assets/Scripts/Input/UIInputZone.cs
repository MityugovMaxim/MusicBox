using System;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIInputZone : UIEntity, IInitializable, IDisposable
{
	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	SignalBus m_SignalBus;

	Animator m_Animator;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelPlaySignal>(RegisterLevelPlay);
		m_SignalBus.Subscribe<LevelExitSignal>(RegisterLevelExit);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelPlaySignal>(RegisterLevelPlay);
		m_SignalBus.Unsubscribe<LevelExitSignal>(RegisterLevelExit);
	}

	void RegisterLevelStart()
	{
		Restore();
	}

	void RegisterLevelPlay()
	{
		Play();
	}

	void RegisterLevelExit()
	{
		Restore();
	}

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	void Play()
	{
		if (m_Animator != null)
			m_Animator.SetTrigger(m_PlayParameterID);
	}

	void Restore()
	{
		if (m_Animator != null)
		{
			m_Animator.ResetTrigger(m_PlayParameterID);
			m_Animator.SetTrigger(m_RestoreParameterID);
		}
	}
}