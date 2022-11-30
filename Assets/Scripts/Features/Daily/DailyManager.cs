using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class DailyManager
{
	public DailyCollection Collection => m_DailyCollection;

	[Inject] AdsProcessor    m_AdsProcessor;
	[Inject] DailyCollection m_DailyCollection;
	[Inject] TimersManager   m_TimersManager;
	[Inject] MenuProcessor   m_MenuProcessor;

	readonly DataEventHandler m_CollectHandler = new DataEventHandler();

	public Task Preload() => m_DailyCollection.Load();

	public void SubscribeCollect(string _DailyID, Action _Action) => m_CollectHandler.AddListener(_DailyID, _Action);

	public void UnsubscribeCollect(string _DailyID, Action _Action) => m_CollectHandler.RemoveListener(_DailyID, _Action);

	public void SubscribeRestore(string _DailyID, Action _Action)
	{
		m_TimersManager.SubscribeCancel(_DailyID, _Action);
		m_TimersManager.SubscribeEnd(_DailyID, _Action);
	}

	public void UnsubscribeRestore(string _DailyID, Action _Action)
	{
		m_TimersManager.UnsubscribeCancel(_DailyID, _Action);
		m_TimersManager.UnsubscribeEnd(_DailyID, _Action);
	}

	public bool IsDailyAvailable(string _DailyID)
	{
		string dailyID = GetDailyID();
		
		if (string.IsNullOrEmpty(dailyID))
			return false;
		
		int sourceOrder = Collection.GetOrder(_DailyID);
		int targetOrder = Collection.GetOrder(dailyID);
		
		return sourceOrder >= targetOrder;
	}

	public List<string> GetDailyIDs()
	{
		return Collection.GetIDs()
			.Where(IsActive)
			.ToList();
	}

	public long GetCoins(string _DailyID)
	{
		DailySnapshot snapshot = Collection.GetSnapshot(_DailyID);
		
		return snapshot?.Coins ?? 0;
	}

	public bool IsAds(string _DailyID)
	{
		DailySnapshot snapshot = Collection.GetSnapshot(_DailyID);
		
		return snapshot?.Ads ?? false;
	}

	public bool IsFree(string _DailyID) => !IsAds(_DailyID);

	public async Task Collect()
	{
		string dailyID = GetDailyID();
		
		if (!IsDailyAvailable(dailyID))
			return;
		
		DailyCollectRequest request = new DailyCollectRequest(dailyID);
		
		bool process = IsFree(dailyID) || await m_AdsProcessor.Rewarded("daily");
		
		if (!process)
		{
			await m_MenuProcessor.ErrorAsync("daily_ads");
			return;
		}
		
		bool success = await request.SendAsync();
		
		if (!success)
		{
			await m_MenuProcessor.ErrorAsync("daily_collect");
			return;
		}
		
		m_CollectHandler.Invoke(dailyID);
	}

	string GetDailyID()
	{
		return Collection.GetIDs()
			.Where(IsActive)
			.Where(IsAvailable)
			.FirstOrDefault();
	}

	bool IsAvailable(string _DailyID)
	{
		if (string.IsNullOrEmpty(_DailyID))
			return false;
		
		if (!m_TimersManager.ContainsTimer(_DailyID))
			return true;
		
		long cooldown  = m_TimersManager.GetEndTimestamp(_DailyID);
		long timestamp = TimeUtility.GetTimestamp();
		
		return timestamp >= cooldown;
	}

	bool IsUnavailable(string _DailyID) => !IsAvailable(_DailyID);

	bool IsActive(string _DailyID)
	{
		DailySnapshot snapshot = Collection.GetSnapshot(_DailyID);
		
		return snapshot?.Active ?? false;
	}

	public long GetTimestamp()
	{
		return Collection.GetIDs()
			.Where(IsActive)
			.Where(IsUnavailable)
			.Select(m_TimersManager.GetEndTimestamp)
			.DefaultIfEmpty(0)
			.Max();
	}
}
