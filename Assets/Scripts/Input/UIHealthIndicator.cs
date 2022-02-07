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

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<HealthChangedSignal>(RegisterHealthChanged);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<HealthChangedSignal>(RegisterHealthChanged);
	}

	void RegisterHealthChanged(HealthChangedSignal _Signal)
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