using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Compression;
using UnityEngine.Scripting;
using Zenject;

public partial class DailyManager : IInitializable, IDisposable
{
	[Inject] ScheduleProcessor m_ScheduleProcessor;

	readonly DataEventHandler m_StartTimer  = new DataEventHandler();
	readonly DataEventHandler m_EndTimer    = new DataEventHandler();
	readonly DataEventHandler m_CancelTimer = new DataEventHandler();
	readonly DataEventHandler m_Collect     = new DataEventHandler();

	public void SubscribeCollect(string _DailyID, Action _Action) => m_Collect.AddListener(_DailyID, _Action);

	public void SubscribeCollect(Action _Action) => m_Collect.AddListener(_Action);

	public void UnsubscribeCollect(string _DailyID, Action _Action) => m_Collect.RemoveListener(_DailyID, _Action);

	public void UnsubscribeCollect(Action _Action) => m_Collect.RemoveListener(_Action);

	public void SubscribeStartTimer(Action _Action) => m_StartTimer.AddListener(_Action);

	public void UnsubscribeStartTimer(Action _Action) => m_StartTimer.RemoveListener(_Action);

	public void SubscribeEndTimer(Action _Action) => m_EndTimer.AddListener(_Action);

	public void UnsubscribeEndTimer(Action _Action) => m_EndTimer.RemoveListener(_Action);

	public void SubscribeCancelTimer(Action _Action) => m_CancelTimer.AddListener(_Action);

	public void UnsubscribeCancelTimer(Action _Action) => m_CancelTimer.RemoveListener(_Action);

	void IInitializable.Initialize()
	{
		Profile.Subscribe(ProcessTimer);
	}

	void IDisposable.Dispose()
	{
		Profile.Unsubscribe(ProcessTimer);
	}

	async void ProcessTimer() => await ProcessTimerAsync();

	Task ProcessTimerAsync()
	{
		m_ScheduleProcessor.Schedule(
			this,
			"daily",
			GetDailyStartTimestamp(),
			GetDailyEndTimestamp(),
			m_StartTimer,
			m_EndTimer,
			m_CancelTimer
		);
		
		return Task.CompletedTask;
	}

	void InvokeCollect(string _DailyID) => m_Collect.Invoke(_DailyID);
}

[Preserve]
public partial class DailyManager : IDataManager
{
	public DailyCollection Collection => m_DailyCollection;

	public ProfileDaily Profile => m_ProfileDaily;

	[Inject] AdsProcessor    m_AdsProcessor;
	[Inject] DailyCollection m_DailyCollection;
	[Inject] ProfileDaily    m_ProfileDaily;

	public Task<bool> Activate()
	{
		return TaskProvider.ProcessAsync(
			this,
			TaskProvider.Group(
				Collection.Load,
				Profile.Load
			),
			TaskProvider.Group(
				ProcessTimerAsync
			)
		);
	}

	public List<string> GetDailyIDs() => Collection.GetIDs().ToList();

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

	public async Task<bool> CollectAsync()
	{
		long timestamp      = TimeUtility.GetTimestamp();
		long startTimestamp = GetDailyStartTimestamp();
		long endTimestamp   = GetDailyEndTimestamp();
		int dailyOrder = timestamp >= startTimestamp && timestamp < endTimestamp
			? GetDailyOrder() + 1
			: 1;
		
		string dailyID = GetDailyID(dailyOrder);
		
		if (IsDailyUnavailable(dailyID))
			return false;
		
		bool process = IsFree(dailyID) || await m_AdsProcessor.Rewarded("daily");
		
		if (!process)
			return false;
		
		DailyCollectRequest request = new DailyCollectRequest();
		
		bool success = await request.SendAsync();
		
		if (!success)
			return false;
		
		InvokeCollect(dailyID);
		
		return true;
	}

	public bool IsDailyAvailable(string _DailyID)
	{
		if (string.IsNullOrEmpty(_DailyID))
			return false;
		
		DailySnapshot snapshot = Collection.GetSnapshot(_DailyID);
		
		if (snapshot == null)
			return false;
		
		long timestamp      = TimeUtility.GetTimestamp();
		long startTimestamp = GetDailyStartTimestamp();
		long endTimestamp   = GetDailyEndTimestamp();
		
		if (timestamp >= startTimestamp && timestamp >= endTimestamp)
			return true;
		
		int sourceOrder = GetDailyOrder();
		int targetOrder = snapshot.Order;
		
		return targetOrder > sourceOrder;
	}

	public bool IsDailyUnavailable(string _DailyID) => !IsDailyAvailable(_DailyID);

	public string GetDailyID(int _DailyOrder)
	{
		DailySnapshot snapshot = Collection.Snapshots.ApproximatelyMax(_Snapshot => _Snapshot.Order, _DailyOrder);
		
		return snapshot?.ID ?? string.Empty;
	}

	public int GetDailyOrder() => Profile.Value?.Order ?? int.MaxValue;

	public long GetDailyStartTimestamp() => Profile.Value?.StartTimestamp ?? 0;

	public long GetDailyEndTimestamp() => Profile.Value?.EndTimestamp ?? 0;
}
