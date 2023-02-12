using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using Zenject;

public enum ChestSlotState
{
	None,
	Pending,
	Processing,
	Ready,
}

[Preserve]
public partial class ChestsManager : IDataManager
{
	public ChestsCollection Collection => m_ChestsCollection;

	public ChestSlots Slots => m_ChestSlots;

	[Inject] ChestSlots            m_ChestSlots;
	[Inject] ProfileBronzeChests   m_ProfileBronzeChests;
	[Inject] ProfileSilverChests   m_ProfileSilverChests;
	[Inject] ProfileGoldChests     m_ProfileGoldChests;
	[Inject] ProfilePlatinumChests m_ProfilePlatinumChests;
	[Inject] ChestsCollection      m_ChestsCollection;

	public Task<bool> Activate()
	{
		return TaskProvider.ProcessAsync(
			this,
			TaskProvider.Group(
				Collection.Load,
				Slots.Load,
				m_ProfileBronzeChests.Load,
				m_ProfileSilverChests.Load,
				m_ProfileGoldChests.Load,
				m_ProfilePlatinumChests.Load
			),
			TaskProvider.Group(ProcessTimers)
		);
	}

	public RankType GetSlotRank(int _Slot)
	{
		ChestSlot snapshot = Slots.GetSnapshot(_Slot);
		
		return snapshot?.Rank ?? RankType.None;
	}

	public long GetSlotStartTimestamp(int _Slot)
	{
		ChestSlot snapshot = Slots.GetSnapshot(_Slot);
		
		return snapshot?.StartTimestamp ?? 0;
	}

	public long GetSlotEndTimestamp(int _Slot)
	{
		ChestSlot snapshot = Slots.GetSnapshot(_Slot);
		
		return snapshot?.EndTimestamp ?? 0;
	}

	public bool TryGetAvailableSlot(out int _Slot)
	{
		const int minSlot = 0;
		const int maxSlot = 3;
		
		for (_Slot = minSlot; _Slot <= maxSlot; _Slot++)
		{
			ChestSlotState state = GetSlotState(_Slot);
			if (state == ChestSlotState.None)
				return true;
		}
		
		_Slot = -1;
		
		return false;
	}

	public ChestSlotState GetSlotState(int _Slot)
	{
		RankType rank = GetSlotRank(_Slot);
		
		if (rank == RankType.None)
			return ChestSlotState.None;
		
		long timestamp      = TimeUtility.GetTimestamp();
		long startTimestamp = GetSlotStartTimestamp(_Slot);
		long endTimestamp   = GetSlotEndTimestamp(_Slot);
		
		if (timestamp < startTimestamp && timestamp < endTimestamp)
			return ChestSlotState.Pending;
		
		if (timestamp >= startTimestamp && timestamp < endTimestamp)
			return ChestSlotState.Processing;
		
		if (timestamp >= startTimestamp && timestamp >= endTimestamp)
			return ChestSlotState.Ready;
		
		return ChestSlotState.None;
	}

	public int GetSlotsCount(ChestSlotState _State)
	{
		return Slots
			.GetSlots()
			.Count(_Slot => GetSlotState(_Slot) == _State);
	}

	public Task UpdateChestsAsync(RankType _Rank)
	{
		switch (_Rank)
		{
			case RankType.Bronze:   return m_ProfileBronzeChests.Reload();
			case RankType.Silver:   return m_ProfileSilverChests.Reload();
			case RankType.Gold:     return m_ProfileGoldChests.Reload();
			case RankType.Platinum: return m_ProfilePlatinumChests.Reload();
			default:                return Task.CompletedTask;
		}
	}

	public int GetChestCapacity(RankType _Rank)
	{
		ChestSnapshot snapshot = Collection.GetSnapshot(_Rank);
		
		return snapshot?.Capacity ?? 0;
	}

	public int GetChestProgress(RankType _Rank)
	{
		switch (_Rank)
		{
			case RankType.Bronze:   return m_ProfileBronzeChests.Progress;
			case RankType.Silver:   return m_ProfileSilverChests.Progress;
			case RankType.Gold:     return m_ProfileGoldChests.Progress;
			case RankType.Platinum: return m_ProfilePlatinumChests.Progress;
			default:                return 0;
		}
	}

	public int GetChestCount() => Enum.GetValues(typeof(RankType)).Cast<RankType>().Sum(GetChestCount);

	public int GetChestCount(RankType _Rank)
	{
		switch (_Rank)
		{
			case RankType.Bronze:   return m_ProfileBronzeChests.Count;
			case RankType.Silver:   return m_ProfileSilverChests.Count;
			case RankType.Gold:     return m_ProfileGoldChests.Count;
			case RankType.Platinum: return m_ProfilePlatinumChests.Count;
			default:                return 0;
		}
	}

	public long GetChestBoost(RankType _Rank)
	{
		ChestSnapshot snapshot = Collection.GetSnapshot(_Rank);
		
		return snapshot?.Boost ?? 0;
	}

	public long GetChestTime(RankType _Rank)
	{
		ChestSnapshot snapshot = Collection.GetSnapshot(_Rank);
		
		return snapshot?.Time ?? 0;
	}

	public RankType GetChestRank(string _ChestID)
	{
		ChestSnapshot snapshot = Collection.GetSnapshot(_ChestID);
		
		return snapshot?.Rank ?? RankType.None;
	}

	public Task<bool> SelectAsync(RankType _Rank, int _Slot)
	{
		if (_Rank == RankType.None)
			return Task.FromResult(false);
		
		ChestSlotState state = GetSlotState(_Slot);
		
		if (state != ChestSlotState.None)
			return Task.FromResult(false);
		
		ChestSelectRequest request = new ChestSelectRequest(_Rank);
		
		return request.SendAsync();
	}

	public Task<ChestReward> OpenAsync(int _Slot)
	{
		ChestSlotState state = GetSlotState(_Slot);
		
		if (state == ChestSlotState.None)
			return Task.FromResult<ChestReward>(null);
		
		ChestOpenRequest request = new ChestOpenRequest(_Slot);
		
		return request.SendAsync();
	}
}
