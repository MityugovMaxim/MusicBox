using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class HealthChangedSignal
{
	public int Health { get; }

	public HealthChangedSignal(int _Health)
	{
		Health = _Health;
	}
}

[Preserve]
public class HealthProcessor : IInitializable, IDisposable
{
	const int MAX_HEALTH = 4;

	readonly SignalBus       m_SignalBus;
	readonly LevelProcessor  m_LevelProcessor;
	readonly LevelController m_LevelController;
	readonly MenuProcessor   m_MenuProcessor;

	string m_LevelID;
	int    m_Health;
	float  m_InvincibilityDuration;
	float  m_InvincibilityTime;

	[Inject]
	public HealthProcessor(
		SignalBus       _SignalBus,
		LevelProcessor  _LevelProcessor,
		LevelController _LevelController,
		MenuProcessor   _MenuProcessor
	)
	{
		m_SignalBus       = _SignalBus;
		m_LevelProcessor  = _LevelProcessor;
		m_LevelController = _LevelController;
		m_MenuProcessor   = _MenuProcessor;
	}

	public void Restore()
	{
		m_Health = MAX_HEALTH;
		
		m_InvincibilityTime = Time.time + m_InvincibilityDuration;
		
		m_SignalBus.Fire(new HealthChangedSignal(m_Health));
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
		
		m_SignalBus.Subscribe<TapFailSignal>(HealthDamage);
		m_SignalBus.Subscribe<DoubleFailSignal>(HealthDamage);
		m_SignalBus.Subscribe<HoldFailSignal>(HealthDamage);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
		
		m_SignalBus.Unsubscribe<TapFailSignal>(HealthDamage);
		m_SignalBus.Unsubscribe<DoubleFailSignal>(HealthDamage);
		m_SignalBus.Unsubscribe<HoldFailSignal>(HealthDamage);
	}

	void RegisterLevelStart(LevelStartSignal _Signal)
	{
		m_LevelID               = _Signal.LevelID;
		m_Health                = MAX_HEALTH;
		m_InvincibilityTime     = 0;
		m_InvincibilityDuration = m_LevelProcessor.GetInvincibility(m_LevelID);
		
		m_SignalBus.Fire(new HealthChangedSignal(m_Health));
	}

	void RegisterLevelRestart(LevelRestartSignal _Signal)
	{
		m_LevelID               = _Signal.LevelID;
		m_Health                = MAX_HEALTH;
		m_InvincibilityTime     = 0;
		m_InvincibilityDuration = m_LevelProcessor.GetInvincibility(m_LevelID);
		
		m_SignalBus.Fire(new HealthChangedSignal(m_Health));
	}

	async void HealthDamage()
	{
		if (Time.time < m_InvincibilityTime)
			return;
		
		m_Health--;
		
		m_InvincibilityTime = Time.time + m_InvincibilityDuration;
		
		m_SignalBus.Fire(new HealthChangedSignal(m_Health));
		
		if (m_Health > 0)
			return;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		m_LevelController.Pause();
		
		await Task.Delay(500);
		
		await m_MenuProcessor.Show(MenuType.ReviveMenu);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}
}