using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class DailyManager
{
	[Inject] DailyCollection m_DailyCollection;
	[Inject] TimersManager   m_TimersManager;

	public string GetDailyID()
	{
		List<string> dailyIDs = m_DailyCollection.GetDailyIDs();
		string       dailyID  = dailyIDs.LastOrDefault();
		for (int i = dailyIDs.Count - 1; i >= 0; i--)
		{
			if (!IsDailyAvailable(dailyIDs[i]))
				break;
			
			dailyID = dailyIDs[i];
		}
		return dailyID;
	}

	public List<string> GetDailyIDs()
	{
		return m_DailyCollection.GetDailyIDs();
	}

	public bool HasDailyAvailable()
	{
		return GetDailyIDs().Any(IsDailyAvailable);
	}

	public bool IsDailyTimer(string _TimerID)
	{
		return m_DailyCollection.Contains(_TimerID);
	}

	public bool IsDailyAvailable(string _DailyID)
	{
		if (!m_TimersManager.Contains(_DailyID))
			return true;
		
		long cooldown  = m_TimersManager.GetEndTimestamp(_DailyID);
		long timestamp = TimeUtility.GetTimestamp();
		
		return timestamp >= cooldown;
	}

	public long GetTimestamp()
	{
		List<string> dailyIDs = GetDailyIDs();
		long timestamp = 0;
		foreach (string dailyID in dailyIDs)
			timestamp = Math.Max(timestamp, GetTimestamp(dailyID));
		return timestamp;
	}

	public long GetTimestamp(string _DailyID)
	{
		return m_TimersManager.Contains(_DailyID) ? m_TimersManager.GetEndTimestamp(_DailyID) : 0;
	}
}
