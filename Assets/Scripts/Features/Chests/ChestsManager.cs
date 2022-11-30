using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ChestsManager : IDataManager
{
	public bool             Activated  { get; private set; }
	public ChestsCollection Collection => m_ChestsCollection;
	public ProfileChests    Profile    => m_ProfileChests;

	[Inject] ChestsCollection      m_ChestsCollection;
	[Inject] ProfileChests         m_ProfileChests;
	[Inject] ProfileCoinsParameter m_ProfileCoins;
	[Inject] TimersManager         m_TimersManager;
	[Inject] MenuProcessor         m_MenuProcessor;

	public async Task<bool> Activate()
	{
		if (Activated)
			return true;
		
		int frame = Time.frameCount;
		
		await Task.WhenAll(
			m_ChestsCollection.Load(),
			m_ProfileChests.Load(),
			m_ProfileCoins.Load()
		);
		
		Activated = true;
		
		return frame == Time.frameCount;
	}

	public List<string> GetChestIDs()
	{
		return Profile.GetIDs()
			.OrderBy(GetType)
			.ToList();
	}

	public List<string> GetAvailableChestIDs()
	{
		return Profile.GetIDs()
			.Where(IsAvailable)
			.OrderBy(GetType)
			.ToList();
	}

	public List<string> GetSelectedChestIDs()
	{
		return Profile.GetIDs()
			.Where(IsSelected)
			.OrderBy(GetSlot)
			.ToList();
	}

	public void SubscribeStart(string _ChestID, Action _Action) => m_TimersManager.SubscribeStart(_ChestID, _Action);

	public void UnsubscribeStart(string _ChestID, Action _Action) => m_TimersManager.UnsubscribeStart(_ChestID, _Action);

	public void SubscribeEnd(string _ChestID, Action _Action) => m_TimersManager.SubscribeEnd(_ChestID, _Action);

	public void UnsubscribeEnd(string _ChestID, Action _Action) => m_TimersManager.UnsubscribeEnd(_ChestID, _Action);

	public ChestType GetType(string _ChestID)
	{
		ProfileChest snapshot = Profile.GetSnapshot(_ChestID);
		
		return snapshot?.Type ?? ChestType.Bronze;
	}

	public ScoreRank GetRank(string _ChestID)
	{
		switch (GetType(_ChestID))
		{
			case ChestType.Bronze:   return ScoreRank.Bronze;
			case ChestType.Silver:   return ScoreRank.Silver;
			case ChestType.Gold:     return ScoreRank.Gold;
			case ChestType.Platinum: return ScoreRank.Platinum;
			default:                 return default;
		}
	}

	public float GetProgress(string _ChestID)
	{
		int count    = GetCount(_ChestID);
		int capacity = GetCapacity(_ChestID);
		
		if (count == 0 || capacity == 0)
			return 0;
		
		return Mathf.InverseLerp(0, capacity, count);
	}

	public long GetOpenTime(string _ChestID) => GetOpenTime(GetType(_ChestID));

	public long GetOpenCoins(string _ChestID) => GetOpenCoins(GetType(_ChestID));

	public long GetStartTimestamp(string _ChestID) => m_TimersManager.GetStartTimestamp(_ChestID);

	public long GetEndTimestamp(string _ChestID) => m_TimersManager.GetEndTimestamp(_ChestID);

	public int GetCount(string _ChestID)
	{
		ProfileChest snapshot = Profile.GetSnapshot(_ChestID);
		
		return snapshot?.Count ?? 0;
	}

	public int GetCapacity(string _ChestID) => GetCapacity(GetType(_ChestID));

	public int GetSlot(string _ChestID)
	{
		ProfileChest snapshot = Profile.GetSnapshot(_ChestID);
		
		return snapshot?.Slot ?? 0;
	}

	public int GetCapacity(ChestType _ChestType)
	{
		ChestSnapshot snapshot = Collection.GetSnapshot(_ChestType);
		
		return snapshot?.Capacity ?? 0;
	}

	public long GetOpenTime(ChestType _ChestType)
	{
		ChestSnapshot snapshot = Collection.GetSnapshot(_ChestType);
		
		return snapshot?.OpenTime ?? 0;
	}

	public long GetOpenCoins(ChestType _ChestType)
	{
		ChestSnapshot snapshot = Collection.GetSnapshot(_ChestType);
		
		return snapshot?.OpenCoins ?? 0;
	}

	public async Task<RequestState> Select(string _ChestID)
	{
		if (IsAvailable(_ChestID))
			return RequestState.None;
		
		ChestSelectRequest request = new ChestSelectRequest(_ChestID);
		
		bool success = await request.SendAsync();
		
		if (!success)
			await m_MenuProcessor.ErrorAsync("chest_select");
		
		return success ? RequestState.Success : RequestState.Fail;
	}

	public async Task<(RequestState, ChestReward)> Boost(string _ChestID)
	{
		if (!IsStarted(_ChestID))
			return (RequestState.None, null);
		
		long coins = GetOpenCoins(_ChestID);
		
		bool payment = await m_ProfileCoins.Remove(coins);
		
		if (!payment)
			return (RequestState.None, null);
		
		ChestBoostRequest request = new ChestBoostRequest(_ChestID);
		
		ChestReward reward = await request.SendAsync();
		
		if (reward != null)
			return (RequestState.Success, reward);
		
		await m_MenuProcessor.ErrorAsync("chest_boost");
		
		return (RequestState.Fail, null);
	}

	public async Task<(RequestState, ChestReward)> Open(string _ChestID)
	{
		if (!IsEnded(_ChestID))
			return (RequestState.None, null);
		
		ChestOpenRequest request = new ChestOpenRequest(_ChestID);
		
		ChestReward reward = await request.SendAsync();
		
		if (reward != null)
			return (RequestState.Success, reward);
		
		await m_MenuProcessor.ErrorAsync("chest_open");
		
		return (RequestState.Fail, null);
	}

	bool IsAvailable(string _ChestID)
	{
		if (IsStarted(_ChestID) || IsEnded(_ChestID))
			return false;
		
		ProfileChest snapshot = Profile.GetSnapshot(_ChestID);
		
		if (snapshot == null)
			return false;
		
		int count    = GetCount(_ChestID);
		int capacity = GetCapacity(_ChestID);
		
		return count >= capacity;
	}

	bool IsSelected(string _ChestID) => IsStarted(_ChestID) || IsEnded(_ChestID);

	public bool IsStarted(string _ChestID)
	{
		ProfileChest snapshot = Profile.GetSnapshot(_ChestID);
		
		if (snapshot == null)
			return false;
		
		long timestamp      = TimeUtility.GetTimestamp();
		long startTimestamp = GetStartTimestamp(_ChestID);
		long endTimestamp   = GetEndTimestamp(_ChestID);
		
		return m_TimersManager.ContainsTimer(_ChestID) && startTimestamp <= timestamp && endTimestamp > timestamp;
	}

	public bool IsEnded(string _ChestID)
	{
		ProfileChest snapshot = Profile.GetSnapshot(_ChestID);
		
		if (snapshot == null)
			return false;
		
		long timestamp      = TimeUtility.GetTimestamp();
		long startTimestamp = GetStartTimestamp(_ChestID);
		long endTimestamp   = GetEndTimestamp(_ChestID);
		
		return m_TimersManager.ContainsTimer(_ChestID) && startTimestamp <= timestamp && endTimestamp <= timestamp;
	}
}
