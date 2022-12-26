using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public partial class ChestsInventory : IDataManager, IInitializable, IDisposable
{
	public ProfileChests Profile => m_ProfileChests;

	[Inject] ProfileChests     m_ProfileChests;
	[Inject] ChestsManager     m_ChestsManager;
	[Inject] ScheduleProcessor m_ScheduleProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;
	[Inject] Localization      m_Localization;

	readonly DataEventHandler m_StartHandler  = new DataEventHandler();
	readonly DataEventHandler m_EndHandler    = new DataEventHandler();
	readonly DataEventHandler m_CancelHandler = new DataEventHandler();

	void IInitializable.Initialize()
	{
		Profile.Subscribe(DataEventType.Add, ProcessTimer);
		Profile.Subscribe(DataEventType.Remove, ProcessTimer);
		Profile.Subscribe(DataEventType.Change, ProcessTimer);
	}

	void IDisposable.Dispose()
	{
		Profile.Unsubscribe(DataEventType.Add, ProcessTimer);
		Profile.Unsubscribe(DataEventType.Remove, ProcessTimer);
		Profile.Unsubscribe(DataEventType.Change, ProcessTimer);
	}

	public Task<bool> Activate()
	{
		return GroupTask.ProcessAsync(
			this,
			GroupTask.CreateGroup(Profile.Load),
			GroupTask.CreateGroup(ProcessTimers)
		);
	}

	public int GetChestsCount(RankType _ChestRank)
	{
		return GetAvailableChestIDs()
			.Select(GetRank)
			.Count(_Rank => _Rank == _ChestRank);
	}

	public string GetAvailableChestID(RankType _ChestRank) => GetChestID(_ChestRank, ChestState.Available);

	public List<string> GetAvailableChestIDs()
	{
		return Profile.GetIDs()
			.Where(IsAvailable)
			.ToList();
	}

	public List<string> GetSelectedChestIDs()
	{
		return Profile.GetIDs()
			.Where(IsSelected)
			.ToList();
	}

	public string GetChestID(int _Slot) => GetSelectedChestIDs().FirstOrDefault(_ChestID => GetSlot(_ChestID) == _Slot);

	public int GetSlot()
	{
		const int count = 4;
		
		for (int slot = 0; slot < count; slot++)
		{
			string chestID = GetChestID(slot);
			if (string.IsNullOrEmpty(chestID))
				return slot;
		}
		
		return -1;
	}

	public int GetSource(string _ChestID)
	{
		ProfileChest snapshot = Profile.GetSnapshot(_ChestID);
		
		return snapshot?.Source ?? 0;
	}

	public int GetSource(RankType _ChestRank) => GetSource(GetChestID(_ChestRank, ChestState.Unavailable));

	public int GetTarget(string _ChestID)
	{
		ProfileChest snapshot = Profile.GetSnapshot(_ChestID);
		
		return snapshot?.Target ?? 0;
	}

	public int GetTarget(RankType _ChestRank)
	{
		string chestID = GetChestID(_ChestRank, ChestState.Unavailable);
		
		return string.IsNullOrEmpty(chestID) ? m_ChestsManager.GetCapacity(_ChestRank) : GetTarget(chestID);
	}

	public float GetProgress(RankType _ChestRank)
	{
		int source = GetSource(_ChestRank);
		int target = GetTarget(_ChestRank);
		
		return MathUtility.Remap01Clamped(source, 0, target); 
	}

	public int GetSlot(string _ChestID)
	{
		ProfileChest snapshot = Profile.GetSnapshot(_ChestID);
		
		if (snapshot == null || snapshot.State != ChestState.Selected)
			return -1;
		
		return snapshot.Slot;
	}

	public long GetStartTimestamp(string _ChestID)
	{
		ProfileChest snapshot = Profile.GetSnapshot(_ChestID);
		
		return snapshot?.StartTimestamp ?? 0;
	}

	public long GetEndTimestamp(string _ChestID)
	{
		ProfileChest snapshot = Profile.GetSnapshot(_ChestID);
		
		return snapshot?.EndTimestamp ?? 0;
	}

	public RankType GetRank(string _ChestID)
	{
		ProfileChest snapshot = Profile.GetSnapshot(_ChestID);
		
		return snapshot?.Rank ?? RankType.None;
	}

	public ChestState GetState(string _ChestID)
	{
		ProfileChest snapshot = Profile.GetSnapshot(_ChestID);
		
		return snapshot?.State ?? ChestState.Unavailable;
	}

	public bool IsUnavailable(string _ChestID) => GetState(_ChestID) == ChestState.Unavailable;

	public bool IsAvailable(string _ChestID) => GetState(_ChestID) == ChestState.Available;

	public bool IsSelected(string _ChestID) => GetState(_ChestID) == ChestState.Selected;

	public bool IsProcessing(string _ChestID)
	{
		ChestState state = GetState(_ChestID);
		
		if (state != ChestState.Selected)
			return false;
		
		long timestamp      = TimeUtility.GetTimestamp();
		long startTimestamp = GetStartTimestamp(_ChestID);
		long endTimestamp   = GetEndTimestamp(_ChestID);
		
		return timestamp >= startTimestamp && timestamp < endTimestamp;
	}

	public bool IsReady(string _ChestID)
	{
		ChestState state = GetState(_ChestID);
		
		if (state != ChestState.Selected)
			return false;
		
		long timestamp      = TimeUtility.GetTimestamp();
		long startTimestamp = GetStartTimestamp(_ChestID);
		long endTimestamp   = GetEndTimestamp(_ChestID);
		
		return timestamp >= startTimestamp && timestamp >= endTimestamp;
	}

	public async Task<RequestState> Select(string _ChestID)
	{
		if (string.IsNullOrEmpty(_ChestID))
			return RequestState.Fail;
		
		if (!IsAvailable(_ChestID))
			return RequestState.Fail;
		
		ChestSelectRequest request = new ChestSelectRequest(_ChestID);
		
		bool success = await request.SendAsync();
		
		if (success)
			return RequestState.Success;
		
		await m_MenuProcessor.ErrorAsync("chest_select");
		
		return RequestState.Fail;
	}

	public async Task<ChestReward> Boost(string _ChestID)
	{
		if (string.IsNullOrEmpty(_ChestID))
			return null;
		
		if (!IsProcessing(_ChestID))
			return null;
		
		RankType rank = GetRank(_ChestID);
		
		long coins = m_ChestsManager.GetBoost(rank);
		
		bool confirm = await m_MenuProcessor.CoinsAsync(
			"chest_boost",
			m_Localization.Get("CHEST_BOOST_TITLE"),
			m_Localization.Get("CHEST_BOOST_MESSAGE"),
			coins
		);
		
		if (!confirm)
			return null;
		
		ChestBoostRequest request = new ChestBoostRequest(_ChestID);
		
		ChestReward reward = await request.SendAsync();
		
		if (reward != null)
			return reward;
		
		await m_MenuProcessor.ErrorAsync("chest_boost");
		
		return null;
	}

	public async Task<ChestReward> Open(string _ChestID)
	{
		if (string.IsNullOrEmpty(_ChestID))
			return null;
		
		if (!IsReady(_ChestID))
			return null;
		
		ChestOpenRequest request = new ChestOpenRequest(_ChestID);
		
		ChestReward reward = await request.SendAsync();
		
		if (reward != null)
			return reward;
		
		await m_MenuProcessor.ErrorAsync("chest_open");
		
		return null;
	}

	string GetChestID(RankType _ChestRank, ChestState _ChestState)
	{
		return Profile.Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Rank == _ChestRank)
			.Where(_Snapshot => _Snapshot.State == _ChestState)
			.Select(_Snapshot => _Snapshot.ID)
			.FirstOrDefault();
	}

	Task ProcessTimers()
	{
		List<string> chestIDs = GetSelectedChestIDs();
		
		if (chestIDs != null && chestIDs.Count > 0)
		{
			foreach (string chestID in chestIDs)
				ProcessTimer(chestID);
		}
		
		return Task.CompletedTask;
	}

	void ProcessTimer(string _ChestID)
	{
		m_ScheduleProcessor.CancelStart(_ChestID);
		m_ScheduleProcessor.CancelEnd(_ChestID);
		
		if (!IsProcessing(_ChestID))
			return;
		
		long startTimestamp = GetStartTimestamp(_ChestID);
		long endTimestamp   = GetEndTimestamp(_ChestID);
		
		m_ScheduleProcessor.ScheduleStart(_ChestID, startTimestamp, m_StartHandler, m_CancelHandler);
		m_ScheduleProcessor.ScheduleEnd(_ChestID, endTimestamp, m_EndHandler, m_CancelHandler);
	}
}
