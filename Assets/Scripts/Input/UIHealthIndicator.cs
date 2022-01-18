using System;
using UnityEngine;
using Zenject;

public class UIHealthIndicator : UIEntity, IInitializable, IDisposable
{
	[SerializeField] UILife[] m_Lives;

	SignalBus       m_SignalBus;
	HealthProcessor m_HealthProcessor;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	public void Initialize()
	{
		m_SignalBus.Subscribe<HealthDamageSignal>(RegisterHealthDamage);
		m_SignalBus.Subscribe<HealthRestoreSignal>(RegisterHealthRestore);
	}

	public void Dispose()
	{
		m_SignalBus.Unsubscribe<HealthDamageSignal>(RegisterHealthDamage);
		m_SignalBus.Unsubscribe<HealthRestoreSignal>(RegisterHealthRestore);
	}

	void RegisterHealthDamage(HealthDamageSignal _Signal)
	{
		ProcessHealth(_Signal.Health);
	}

	void RegisterHealthRestore(HealthRestoreSignal _Signal)
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