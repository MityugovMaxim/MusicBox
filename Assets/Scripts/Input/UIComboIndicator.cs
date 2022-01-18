using System;
using UnityEngine;
using Zenject;

public class UIComboIndicator : UIGroup, IInitializable, IDisposable
{
	[SerializeField] UIUnitLabel m_Label;

	SignalBus m_SignalBus;

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
		m_Label.Value = _Signal.Combo;
		
		if (_Signal.Combo > 0)
			Show();
		else
			Hide();
	}

	void Restore()
	{
		Hide(true);
		
		m_Label.Value = 0;
	}
}