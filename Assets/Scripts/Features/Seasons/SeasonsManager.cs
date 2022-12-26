using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public enum SeasonItemMode
{
	None = 0,
	Free = 1,
	Paid = 2,
}

[Preserve]
public class SeasonsManager : IDataManager
{
	public SeasonsCollection Collection => m_SeasonsCollection;

	public ProfileSeasons Profile => m_ProfileSeasons;

	public SeasonsDescriptor Descriptor => m_SeasonsDescriptor;

	[Inject] SeasonsCollection m_SeasonsCollection;
	[Inject] SeasonsDescriptor m_SeasonsDescriptor;
	[Inject] ProfileSeasons    m_ProfileSeasons;
	[Inject] ProductsManager   m_ProductsManager;

	public void SubscribePass(string _SeasonID, Action _Action) => m_ProductsManager.Profile.SubscribePurchase(_SeasonID, _Action);

	public void UnsubscribePass(string _SeasonID, Action _Action) => m_ProductsManager.Profile.UnsubscribePurchase(_SeasonID, _Action);

	public Task<bool> Activate()
	{
		return GroupTask.ProcessAsync(
			this,
			Collection.Load,
			Descriptor.Load,
			Profile.Load
		);
	}

	public string GetSeasonID()
	{
		long timestamp = TimeUtility.GetTimestamp();
		
		return Collection.Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.StartTimestamp <= timestamp)
			.Where(_Snapshot => _Snapshot.EndTimestamp >= timestamp)
			.Select(_Snapshot => _Snapshot.ID)
			.FirstOrDefault();
	}

	public int GetAvailableLevel(string _SeasonID)
	{
		int level = GetLevel(_SeasonID);
		
		return GetLevels(_SeasonID)
			.OrderBy(_Level => _Level)
			.Where(_Level => _Level <= level)
			.Where(_Level => IsItemAvailable(_SeasonID, _Level))
			.DefaultIfEmpty(level)
			.FirstOrDefault();
	}

	public async Task<RequestState> Collect(string _SeasonID, int _Level, SeasonItemMode _Mode)
	{
		if (!IsItemAvailable(_SeasonID, _Level, _Mode))
			return RequestState.Fail;
		
		SeasonCollectRequest request = new SeasonCollectRequest(_SeasonID, _Level, _Mode);
		
		bool success = await request.SendAsync();
		
		return success ? RequestState.Success : RequestState.Fail;
	}

	public async Task<RequestState> Complete()
	{
		long timestamp = TimeUtility.GetTimestamp();
		
		List<string> seasonIDs = Profile.GetIDs()
			.Where(_SeasonID => GetStartTimestamp(_SeasonID) <= timestamp)
			.Where(_SeasonID => GetEndTimestamp(_SeasonID) <= timestamp)
			.ToList();
		
		foreach (string seasonID in seasonIDs)
		{
			if (string.IsNullOrEmpty(seasonID))
				continue;
			
			SeasonCompleteRequest request = new SeasonCompleteRequest(seasonID);
			
			await request.SendAsync();
		}
		
		return RequestState.Success;
	}

	public bool HasPass(string _SeasonID) => m_ProductsManager.Profile.Contains(_SeasonID);

	public string GetTitle(string _SeasonID) => m_SeasonsDescriptor.GetTitle(_SeasonID);

	public List<int> GetLevels(string _SeasonID)
	{
		SeasonSnapshot snapshot = Collection.GetSnapshot(_SeasonID);
		
		if (snapshot?.Levels == null || snapshot.Levels.Count == 0)
			return null;
		
		return snapshot.Levels
			.Where(_Entry => _Entry != null)
			.Select(_Entry => _Entry.Level)
			.ToList();
	}

	public int GetMinLevel(string _SeasonID)
	{
		SeasonSnapshot snapshot = Collection.GetSnapshot(_SeasonID);
		
		if (snapshot?.Levels == null || snapshot.Levels.Count == 0)
			return int.MinValue;
		
		return snapshot.Levels
			.Where(_Entry => _Entry != null)
			.Select(_Entry => _Entry.Level)
			.DefaultIfEmpty(int.MinValue)
			.Min();
	}

	public int GetMaxLevel(string _SeasonID)
	{
		SeasonSnapshot snapshot = Collection.GetSnapshot(_SeasonID);
		
		if (snapshot?.Levels == null || snapshot.Levels.Count == 0)
			return int.MaxValue;
		
		return snapshot.Levels
			.Where(_Entry => _Entry != null)
			.Select(_Entry => _Entry.Level)
			.DefaultIfEmpty(int.MaxValue)
			.Max();
	}

	public long GetCoins(string _SeasonID, int _Level, SeasonItemMode _Mode)
	{
		SeasonItem item = GetItem(_SeasonID, _Level, _Mode);
		
		return item?.Coins ?? 0;
	}

	public string GetVoucherID(string _SeasonID, int _Level, SeasonItemMode _Mode)
	{
		SeasonItem item = GetItem(_SeasonID, _Level, _Mode);
		
		return item?.VoucherID ?? string.Empty;
	}

	public string GetSongID(string _SeasonID, int _Level, SeasonItemMode _Mode)
	{
		SeasonItem item = GetItem(_SeasonID, _Level, _Mode);
		
		return item?.SongID ?? string.Empty;
	}

	public string GetChestID(string _SeasonID, int _Level, SeasonItemMode _Mode)
	{
		SeasonItem item = GetItem(_SeasonID, _Level, _Mode);
		
		return item?.ChestID ?? string.Empty;
	}

	public long GetPoints(string _SeasonID)
	{
		ProfileSeason snapshot = Profile.GetSnapshot(_SeasonID);
		
		return snapshot?.Points ?? 0;
	}

	public int GetLevel(string _SeasonID)
	{
		SeasonSnapshot snapshot = Collection.GetSnapshot(_SeasonID);
		
		if (snapshot == null)
			return 0;
		
		long points = GetPoints(_SeasonID);
		
		return snapshot.Levels
			.Where(_Level => _Level != null)
			.Where(_Level => _Level.Points <= points)
			.Select(_Level => _Level.Level)
			.Max();
	}

	public float GetProgress(string _SeasonID, int _Level)
	{
		int minLevel    = GetMinLevel(_SeasonID);
		int maxLevel    = GetMaxLevel(_SeasonID);
		int sourceLevel = Mathf.Clamp(_Level, minLevel, maxLevel);
		int targetLevel = Mathf.Clamp(_Level + 1, minLevel, maxLevel);
		
		if (sourceLevel == targetLevel)
			return 1;
		
		if (_Level > targetLevel)
			return 1;
		
		if (_Level < sourceLevel)
			return 0;
		
		SeasonLevel source = GetLevel(_SeasonID, sourceLevel);
		SeasonLevel target = GetLevel(_SeasonID, targetLevel);
		
		long sourcePoints = source?.Points ?? 0;
		long targetPoints = target?.Points ?? 0;
		
		long points = GetPoints(_SeasonID);
		
		if (points >= targetPoints)
			return 1;
		
		if (points <= sourcePoints)
			return 0;
		
		return MathUtility.Remap01(points, sourcePoints, targetPoints);
	}

	public long GetStartTimestamp(string _SeasonID)
	{
		SeasonSnapshot snapshot = Collection.GetSnapshot(_SeasonID);
		
		return snapshot?.StartTimestamp ?? 0;
	}

	public long GetEndTimestamp(string _SeasonID)
	{
		SeasonSnapshot snapshot = Collection.GetSnapshot(_SeasonID);
		
		return snapshot?.EndTimestamp ?? 0;
	}

	public bool IsLevelAvailable(string _SeasonID, int _Level)
	{
		int level = GetLevel(_SeasonID);
		
		return level >= _Level;
	}

	public bool IsLevelUnavailable(string _SeasonID, int _Level) => !IsLevelAvailable(_SeasonID, _Level);

	public bool IsItemAvailable(string _SeasonID, int _Level) => IsItemAvailable(_SeasonID, _Level, SeasonItemMode.Free) || IsItemAvailable(_SeasonID, _Level, SeasonItemMode.Paid);

	public bool IsItemAvailable(string _SeasonID, int _Level, SeasonItemMode _Mode)
	{
		int level = GetLevel(_SeasonID);
		
		if (level < _Level)
			return false;
		
		if (_Mode == SeasonItemMode.Paid && !HasPass(_SeasonID))
			return false;
		
		ProfileSeason snapshot = Profile.GetSnapshot(_SeasonID);
		
		List<int> items;
		switch (_Mode)
		{
			case SeasonItemMode.Free:
				items = snapshot?.FreeItems;
				break;
			case SeasonItemMode.Paid:
				items = snapshot?.PaidItems;
				break;
			default:
				items = null;
				break;
		}
		
		return items == null || !items.Contains(_Level);
	}

	public bool IsItemUnavailable(string _SeasonID, int _Level, SeasonItemMode _Mode) => !IsItemAvailable(_SeasonID, _Level, _Mode);

	SeasonLevel GetLevel(string _SeasonID, int _Level)
	{
		SeasonSnapshot snapshot = Collection.GetSnapshot(_SeasonID);
		
		if (snapshot?.Levels == null || snapshot.Levels.Count == 0)
			return null;
		
		return snapshot.Levels
			.Where(_Entry => _Entry != null)
			.FirstOrDefault(_Entry => _Entry.Level == _Level);
	}

	SeasonItem GetItem(string _SeasonID, int _Level, SeasonItemMode _Mode)
	{
		SeasonLevel level = GetLevel(_SeasonID, _Level);
		
		if (level == null)
			return null;
		
		switch (_Mode)
		{
			case SeasonItemMode.Free: return level.FreeItem;
			case SeasonItemMode.Paid: return level.PaidItem;
			default:                  return null;
		}
	}
}
