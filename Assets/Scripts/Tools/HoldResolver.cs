using System;
using UnityEngine;
using Zenject;

public class HoldResolver : GotoResolver, IInitializable, IDisposable
{
	[SerializeField] int m_HoldTarget;

	SignalBus m_SignalBus;

	int m_HoldCount;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<HoldSuccessSignal>(RegisterHoldSuccess);
		m_SignalBus.Subscribe<SongRestartSignal>(RegisterLevelRestart);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<HoldSuccessSignal>(RegisterHoldSuccess);
		m_SignalBus.Unsubscribe<SongRestartSignal>(RegisterLevelRestart);
	}

	public override bool Resolve()
	{
		return m_HoldCount >= m_HoldTarget;
	}

	void RegisterHoldSuccess()
	{
		m_HoldCount++;
	}

	void RegisterLevelRestart()
	{
		m_HoldCount = 0;
		
		Hide(true);
	}
}