using UnityEngine;
using Zenject;

public class UIDailyTimer : UIEntity
{
	[SerializeField] UIAnalogTimer m_Timer;

	[Inject] DailyManager m_DailyManager;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessTimer();
		
		m_DailyManager.SubscribeCollect(ProcessTimer);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_DailyManager.UnsubscribeCollect(ProcessTimer);
	}

	void ProcessTimer()
	{
		long timestamp = m_DailyManager.GetTimestamp();
		
		m_Timer.Setup(timestamp);
	}
}
