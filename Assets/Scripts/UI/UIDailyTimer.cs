using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIDailyTimer : UIEntity
{
	[SerializeField] UIAnalogTimer m_Timer;

	[Inject] DailyManager m_DailyManager;

	readonly List<string> m_DailyIDs = new List<string>();

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessTimer();
		
		m_DailyIDs.Clear();
		m_DailyIDs.AddRange(m_DailyManager.GetDailyIDs());
		
		foreach (string dailyID in m_DailyIDs)
			m_DailyManager.SubscribeCollect(dailyID, ProcessTimer);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		foreach (string dailyID in m_DailyIDs)
			m_DailyManager.UnsubscribeCollect(dailyID, ProcessTimer);
		
		m_DailyIDs.Clear();
	}

	void ProcessTimer()
	{
		long startTimestamp = m_DailyManager.GetDailyStartTimestamp();
		long endTimestamp   = m_DailyManager.GetDailyEndTimestamp();
		
		m_Timer.SetTimer(startTimestamp, endTimestamp);
	}
}
