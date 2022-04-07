using System;
using UnityEngine;
using Zenject;

public class UIHealthIndicator : UIEntity, IInitializable, IDisposable
{
	[SerializeField] UILife[] m_Lives;

	[Inject] SignalBus m_SignalBus;

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<HealthRestoreSignal>(RegisterHealthRestore);
		m_SignalBus.Subscribe<HealthDecreaseSignal>(RegisterHealthDecrease);
		m_SignalBus.Subscribe<HealthIncreaseSignal>(RegisterHealthIncrease);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<HealthRestoreSignal>(RegisterHealthRestore);
		m_SignalBus.Unsubscribe<HealthDecreaseSignal>(RegisterHealthDecrease);
		m_SignalBus.Unsubscribe<HealthIncreaseSignal>(RegisterHealthIncrease);
	}

	void RegisterHealthRestore(HealthRestoreSignal _Signal)
	{
		ProcessHealth(_Signal.Health);
	}

	void RegisterHealthDecrease(HealthDecreaseSignal _Signal)
	{
		ProcessHealth(_Signal.Health);
	}

	void RegisterHealthIncrease(HealthIncreaseSignal _Signal)
	{
		ProcessHealth(_Signal.Health);
	}

	void ProcessHealth(int _Health)
	{
		for (int i = 0; i < m_Lives.Length; i++)
		{
			if (m_Lives[i] != null)
				m_Lives[i].SetActive(i < _Health);
		}
	}
}