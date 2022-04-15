using UnityEngine;
using Zenject;

public class UIHealthIndicator : UIEntity
{
	[SerializeField] UIHealthHandle[] m_Handles;

	[Inject] SignalBus m_SignalBus;

	int m_Health;

	protected override void Awake()
	{
		base.Awake();
		
		m_SignalBus.Subscribe<HealthSignal>(RegisterHealth);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_SignalBus.Unsubscribe<HealthSignal>(RegisterHealth);
	}

	void RegisterHealth(HealthSignal _Signal)
	{
		ProcessHealth(_Signal.Health);
	}

	void ProcessHealth(int _Health)
	{
		for (int i = 0; i < m_Handles.Length; i++)
		{
			UIHealthHandle handle = m_Handles[i];
			
			if (handle == null)
				continue;
			
			if (i < _Health)
				handle.Restore();
			else
				handle.Damage();
		}
	}
}