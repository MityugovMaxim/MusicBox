using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using UnityEngine;
using Zenject;

public class DailySnapshot : Snapshot
{
	public bool Active   { get; }
	public long Cooldown { get; }
	public long Coins    { get; }
	public bool Ads      { get; }

	public DailySnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active   = _Data.GetBool("active");
		Cooldown = _Data.GetLong("cooldown");
		Coins    = _Data.GetLong("coins");
		Ads      = _Data.GetBool("ads");
	}
}

public class DailyDataUpdateSignal { }

public class DailyProcessor : DataProcessor<DailySnapshot, DailyDataUpdateSignal>
{
	protected override string Path => "daily";

	public List<string> GetDailyIDs()
	{
		return Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public long GetCooldown(string _DailyID)
	{
		DailySnapshot snapshot = GetSnapshot(_DailyID);
		
		return snapshot?.Cooldown ?? 0;
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

public class DailyManager
{
	[Inject] ProfileProcessor m_ProfileProcessor;
	[Inject] DailyProcessor   m_DailyProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;

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

	public bool IsDailyAvailable(string _DailyID)
	{
		ProfileTimer timer = m_ProfileProcessor.GetTimer(_DailyID);
		
		if (timer == null)
			return true;
		
		long cooldown  = timer.EndTimestamp;
		long timestamp = TimeUtility.GetTimestamp();
		
		return timestamp >= cooldown;
	}

	public long GetStartTimestamp()
	{
		List<string> dailyIDs = GetDailyIDs();
		long timestamp = 0;
		foreach (string dailyID in dailyIDs)
			timestamp = Math.Max(timestamp, GetStartTimestamp(dailyID));
		return timestamp;
	}

	public long GetEndTimestamp()
	{
		List<string> dailyIDs = GetDailyIDs();
		long timestamp = 0;
		foreach (string dailyID in dailyIDs)
			timestamp = Math.Max(timestamp, GetEndTimestamp(dailyID));
		return timestamp;
	}

	public long GetStartTimestamp(string _DailyID)
	{
		ProfileTimer timer = m_ProfileProcessor.GetTimer(_DailyID);
		
		return timer?.StartTimestamp ?? 0;
	}

	public long GetEndTimestamp(string _DailyID)
	{
		ProfileTimer timer = m_ProfileProcessor.GetTimer(_DailyID);
		
		return timer?.EndTimestamp ?? 0;
	}
}

public class DailyCollectRequest : FunctionRequest<bool>
{
	protected override string Command => "DailyCollect";

	string DailyID { get; }

	public DailyCollectRequest(string _DailyID)
	{
		DailyID = _DailyID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["daily_id"] = DailyID;
	}

	protected override bool Success(object _Data)
	{
		return _Data != null && (bool)_Data;
	}

	protected override bool Fail()
	{
		return false;
	}
}