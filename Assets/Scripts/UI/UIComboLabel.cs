using System;
using TMPro;
using UnityEngine;
using Zenject;

public class UIComboLabel : UIEntity, IInitializable, IDisposable
{
	[SerializeField] TMP_Text m_Label;

	SignalBus m_SignalBus;

	int m_Multiplier;

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
		SetMultiplier(0);
	}

	void RegisterLevelRestart()
	{
		SetMultiplier(0);
	}

	void RegisterLevelCombo(LevelComboSignal _Signal)
	{
		SetMultiplier(_Signal.Multiplier);
	}

	void SetMultiplier(int _Multiplier)
	{
		m_Multiplier = _Multiplier;
		
		m_Label.gameObject.SetActive(m_Multiplier > 1);
		m_Label.text = $"×{m_Multiplier}";
	}
}