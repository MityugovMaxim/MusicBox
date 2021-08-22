using System;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIInputZone : UIEntity, IInitializable, IDisposable
{
	static readonly int m_PlayParameterID      = Animator.StringToHash("Play");
	static readonly int m_RestoreParameterID   = Animator.StringToHash("Restore");
	static readonly int m_HighlightParameterID = Animator.StringToHash("Highlight");

	SignalBus m_SignalBus;

	Animator m_Animator;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
		m_Animator  = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Subscribe<LevelPlaySignal>(RegisterLevelPlay);
		m_SignalBus.Subscribe<LevelExitSignal>(RegisterLevelExit);
		m_SignalBus.Subscribe<LevelComboSignal>(RegisterLevelCombo);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Unsubscribe<LevelPlaySignal>(RegisterLevelPlay);
		m_SignalBus.Unsubscribe<LevelExitSignal>(RegisterLevelExit);
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

	void RegisterLevelPlay()
	{
		Play();
	}

	void RegisterLevelExit()
	{
		Restore();
	}

	void RegisterLevelCombo(LevelComboSignal _Signal)
	{
		Highlight(_Signal.Multiplier >= 4);
	}

	void Play()
	{
		m_Animator.SetTrigger(m_PlayParameterID);
	}

	void Highlight(bool _Value)
	{
		m_Animator.SetBool(m_HighlightParameterID, _Value);
	}

	void Restore()
	{
		m_Animator.ResetTrigger(m_PlayParameterID);
		m_Animator.SetBool(m_HighlightParameterID, false);
		m_Animator.SetTrigger(m_RestoreParameterID);
	}
}