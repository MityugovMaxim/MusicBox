using System;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIComboMultiplier : UIEntity, IInitializable, IDisposable
{
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_X2ParameterID      = Animator.StringToHash("X2");
	static readonly int m_X3ParameterID      = Animator.StringToHash("X3");
	static readonly int m_X4ParameterID      = Animator.StringToHash("X4");

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
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Subscribe<LevelComboSignal>(RegisterLevelCombo);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Unsubscribe<LevelComboSignal>(RegisterLevelCombo);
	}

	void RegisterLevelStart()
	{
		Restore();
	}

	void RegisterLevelRestart()
	{
		Restore();
	}

	void RegisterLevelCombo(LevelComboSignal _Signal)
	{
		SetMultiplier(_Signal.Multiplier);
	}

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	void Restore()
	{
		m_Animator.ResetTrigger(m_X2ParameterID);
		m_Animator.ResetTrigger(m_X3ParameterID);
		m_Animator.ResetTrigger(m_X4ParameterID);
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	void SetMultiplier(int _Multiplier)
	{
		switch (_Multiplier)
		{
			case 2:
				m_Animator.SetTrigger(m_X2ParameterID);
				break;
			case 3:
				m_Animator.SetTrigger(m_X3ParameterID);
				break;
			case 4:
				m_Animator.SetTrigger(m_X4ParameterID);
				break;
		}
	}
}