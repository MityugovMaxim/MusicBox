using System;
using UnityEngine;
using Zenject;

public class DoubleResolver : GotoResolver, IInitializable, IDisposable
{
	[SerializeField] int m_DoubleTarget;

	SignalBus m_SignalBus;

	int m_DoubleCount;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<DoubleSuccessSignal>(RegisterDoubleSuccess);
		m_SignalBus.Subscribe<SongRestartSignal>(RegisterLevelRestart);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<DoubleSuccessSignal>(RegisterDoubleSuccess);
		m_SignalBus.Unsubscribe<SongRestartSignal>(RegisterLevelRestart);
	}

	public override bool Resolve()
	{
		return m_DoubleCount >= m_DoubleTarget;
	}

	void RegisterDoubleSuccess()
	{
		m_DoubleCount++;
	}

	void RegisterLevelRestart()
	{
		m_DoubleCount = 0;
		
		Hide(true);
	}
}