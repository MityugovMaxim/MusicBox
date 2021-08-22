using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIComboMultiplier : UIEntity
{
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_ShowParameterID    = Animator.StringToHash("Show");

	[SerializeField] int m_Multiplier;

	SignalBus m_SignalBus;

	Animator m_Animator;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Subscribe<LevelComboSignal>(RegisterLevelCombo);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
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
		m_Animator.SetBool(m_ShowParameterID, m_Multiplier == _Signal.Multiplier);
	}

	void Restore()
	{
		m_Animator.SetBool(m_ShowParameterID, false);
		m_Animator.SetTrigger(m_RestoreParameterID);
	}
}
