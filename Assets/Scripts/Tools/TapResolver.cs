using System;
using UnityEngine;
using Zenject;

public class TapResolver : GotoResolver, IInitializable, IDisposable
{
	[SerializeField] int m_TapTarget;

	SignalBus m_SignalBus;

	bool m_Shown;
	int  m_TapCount;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<TapSuccessSignal>(RegisterTapSuccess);
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<TapSuccessSignal>(RegisterTapSuccess);
		m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
	}

	public override bool Resolve()
	{
		return m_TapCount >= m_TapTarget;
	}

	void RegisterTapSuccess()
	{
		m_TapCount++;
	}

	void RegisterLevelRestart()
	{
		m_TapCount = 0;
		
		Hide(true);
	}
}