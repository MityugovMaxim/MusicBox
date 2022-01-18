using System;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class HealthDamageSignal
{
	public int Health { get; }

	public HealthDamageSignal(int _Health)
	{
		Health = _Health;
	}
}

public class HealthRestoreSignal
{
	public int Health { get; }

	public HealthRestoreSignal(int _Health)
	{
		Health = _Health;
	}
}

[Preserve]
public class HealthProcessor : IInitializable, IDisposable
{
	const int MAX_HEALTH = 4;

	readonly SignalBus      m_SignalBus;
	readonly LevelProcessor m_LevelProcessor;
	readonly MenuProcessor  m_MenuProcessor;

	string m_LevelID;
	int    m_Combo;
	int    m_Health;
	float  m_InvincibilityDuration;
	float  m_InvincibilityTime;

	public HealthProcessor(
		SignalBus      _SignalBus,
		LevelProcessor _LevelProcessor,
		MenuProcessor  _MenuProcessor
	)
	{
		m_SignalBus      = _SignalBus;
		m_LevelProcessor = _LevelProcessor;
		m_MenuProcessor  = _MenuProcessor;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Subscribe<LevelComboSignal>(RegisterLevelCombo);
		m_SignalBus.Subscribe<LevelReviveSignal>(RegisterLevelRevive);
		
		m_SignalBus.Subscribe<TapFailSignal>(RegisterDoubleFail);
		m_SignalBus.Subscribe<DoubleFailSignal>(RegisterTapFail);
		m_SignalBus.Subscribe<HoldFailSignal>(RegisterHoldFail);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Unsubscribe<LevelComboSignal>(RegisterLevelCombo);
		m_SignalBus.Unsubscribe<LevelReviveSignal>(RegisterLevelRevive);
		
		m_SignalBus.Unsubscribe<TapFailSignal>(RegisterDoubleFail);
		m_SignalBus.Unsubscribe<DoubleFailSignal>(RegisterTapFail);
		m_SignalBus.Unsubscribe<HoldFailSignal>(RegisterHoldFail);
	}

	void RegisterLevelStart(LevelStartSignal _Signal)
	{
		m_LevelID               = _Signal.LevelID;
		m_Health                = MAX_HEALTH;
		m_Combo                 = 0;
		m_InvincibilityTime     = 0;
		m_InvincibilityDuration = m_LevelProcessor.GetInvincibility(m_LevelID);
		
		m_SignalBus.Fire(new HealthRestoreSignal(m_Health));
	}

	void RegisterLevelRestart(LevelRestartSignal _Signal)
	{
		m_LevelID               = _Signal.LevelID;
		m_Health                = MAX_HEALTH;
		m_Combo                 = 0;
		m_InvincibilityTime     = 0;
		m_InvincibilityDuration = m_LevelProcessor.GetInvincibility(m_LevelID);
		
		m_SignalBus.Fire(new HealthRestoreSignal(m_Health));
	}

	void RegisterDoubleFail()
	{
		HealthDamage();
	}

	void RegisterTapFail()
	{
		HealthDamage();
	}

	void RegisterHoldFail()
	{
		HealthDamage();
	}

	void RegisterLevelCombo(LevelComboSignal _Signal)
	{
		if (m_Combo < _Signal.Multiplier)
			HealthRestore();
		
		m_Combo = _Signal.Multiplier;
	}

	void RegisterLevelRevive()
	{
		m_Health = Mathf.Clamp(m_Health, 1, MAX_HEALTH);
		
		m_InvincibilityTime = Time.time + m_InvincibilityDuration * 2;
		
		m_SignalBus.Fire(new HealthRestoreSignal(m_Health));
	}

	void HealthDamage()
	{
		if (Time.time < m_InvincibilityTime)
			return;
		
		m_Health--;
		
		m_InvincibilityTime = Time.time + m_InvincibilityDuration;
		
		m_SignalBus.Fire(new HealthDamageSignal(m_Health));
		
		if (m_Health > 0)
			return;
		
		m_LevelProcessor.Pause();
		
		m_MenuProcessor.Show(MenuType.ReviveMenu);
	}

	void HealthRestore()
	{
		if (m_Health >= MAX_HEALTH)
			return;
		
		m_Health++;
		
		m_SignalBus.Fire(new HealthRestoreSignal(m_Health));
	}
}