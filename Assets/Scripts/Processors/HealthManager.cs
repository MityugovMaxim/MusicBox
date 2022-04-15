using System;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class HealthSignal
{
	public int Health { get; }

	public HealthSignal(int _Health)
	{
		Health = _Health;
	}
}

[Preserve]
public class HealthManager : IInitializable, IDisposable
{
	const int MAX_HEALTH = 4;

	[Inject] SignalBus      m_SignalBus;
	[Inject] SongsProcessor m_SongsProcessor;

	string m_SongID;
	Action m_Death;
	int    m_Health;
	float  m_InvincibilityDuration;
	float  m_InvincibilityTime;

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<TapFailSignal>(Damage);
		m_SignalBus.Subscribe<DoubleFailSignal>(Damage);
		m_SignalBus.Subscribe<HoldFailSignal>(Damage);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<TapFailSignal>(Damage);
		m_SignalBus.Unsubscribe<DoubleFailSignal>(Damage);
		m_SignalBus.Unsubscribe<HoldFailSignal>(Damage);
	}

	public void Setup(string _SongID, Action _Death)
	{
		m_SongID = _SongID;
		m_Death  = _Death;
		
		m_InvincibilityTime     = 0;
		m_InvincibilityDuration = m_SongsProcessor.GetInvincibility(m_SongID);
	}

	public void Restore()
	{
		m_Health = MAX_HEALTH;
		
		m_InvincibilityTime = 0;
		
		m_SignalBus.Fire(new HealthSignal(m_Health));
	}

	void Damage()
	{
		if (Time.time < m_InvincibilityTime)
			return;
		
		m_InvincibilityTime = Time.time + m_InvincibilityDuration;
		
		m_Health = Mathf.Max(0, m_Health - 1);
		
		m_SignalBus.Fire(new HealthSignal(m_Health));
		
		if (m_Health == 0)
			m_Death?.Invoke();
	}
}
