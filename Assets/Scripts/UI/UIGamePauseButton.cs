using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIGamePauseButton : UIEntity
{
	static readonly int m_ShowParameterID    = Animator.StringToHash("Show");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	SignalBus   m_SignalBus;
	UIPauseMenu m_PauseMenu;

	bool     m_Shown;
	Animator m_Animator;

	[Inject]
	public void Construct(
		SignalBus   _SignalBus,
		UIPauseMenu _PauseMenu
	)
	{
		m_SignalBus = _SignalBus;
		m_PauseMenu = _PauseMenu;
		
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
	}

	void RegisterLevelStart()
	{
		Restore();
	}

	void RegisterLevelRestart()
	{
		m_Shown = false;
		m_Animator.ResetTrigger(m_RestoreParameterID);
		m_Animator.SetBool(m_ShowParameterID, false);
	}

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	public void Pause()
	{
		m_Shown = !m_Shown;
		
		if (m_Shown)
		{
			m_PauseMenu.Pause();
			m_Animator.SetBool(m_ShowParameterID, true);
		}
		else
		{
			m_PauseMenu.Resume();
			m_Animator.SetBool(m_ShowParameterID, false);
		}
	}

	void Restore()
	{
		m_Shown = false;
		m_Animator.SetBool(m_ShowParameterID, false);
		m_Animator.SetTrigger(m_RestoreParameterID);
	}
}
