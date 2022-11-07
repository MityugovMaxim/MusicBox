using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

public class DailySnapshot : Snapshot
{
	public bool Active   { get; }
	public long Cooldown { get; }
	public long Coins    { get; }
	public bool Ads      { get; }

	public DailySnapshot() : base("new_daily", 0)
	{
		Active   = false;
		Cooldown = 60000;
		Coins    = 0;
		Ads      = false;
	}

	public DailySnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active   = _Data.GetBool("active");
		Cooldown = _Data.GetLong("cooldown");
		Coins    = _Data.GetLong("coins");
		Ads      = _Data.GetBool("ads");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["active"]   = Active;
		_Data["cooldown"] = Cooldown;
		_Data["coins"]    = Coins;
		_Data["ads"]      = Ads;
	}
}

[Preserve]
public class DailyDataUpdateSignal { }

[Preserve]
public class DailyProcessor : DataProcessor<DailySnapshot, DailyDataUpdateSignal>
{
	protected override string Path => "daily";

	protected override bool SupportsDevelopment => true;

	public List<string> GetDailyIDs()
	{
		return Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public long GetCoins(string _DailyID)
	{
		DailySnapshot snapshot = GetSnapshot(_DailyID);
		
		return snapshot?.Coins ?? 0;
	}

	public bool GetAds(string _DailyID)
	{
		DailySnapshot snapshot = GetSnapshot(_DailyID);
		
		return snapshot?.Ads ?? false;
	}
}

[Preserve]
public class DailyManager
{
	[Inject] DailyProcessor  m_DailyProcessor;
	[Inject] TimersProcessor m_TimersProcessor;

	public string GetDailyID()
	{
		List<string> dailyIDs = m_DailyProcessor.GetDailyIDs();
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
		return m_DailyProcessor.GetDailyIDs();
	}

	public bool HasDailyAvailable()
	{
		return GetDailyIDs().Any(IsDailyAvailable);
	}

	public bool IsDailyTimer(string _TimerID)
	{
		return m_DailyProcessor.Contains(_TimerID);
	}

	public bool IsDailyAvailable(string _DailyID)
	{
		if (!m_TimersProcessor.Contains(_DailyID))
			return true;
		
		long cooldown  = m_TimersProcessor.GetEndTimestamp(_DailyID);
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
		return m_TimersProcessor.Contains(_DailyID) ? m_TimersProcessor.GetEndTimestamp(_DailyID) : 0;
	}
}
