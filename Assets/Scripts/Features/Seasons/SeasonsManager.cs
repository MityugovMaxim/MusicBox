using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class SeasonsManager : IDataManager
{
	public bool Activated { get; private set; }

	public SeasonsCollection Collection => m_SeasonsCollection;

	public ProfileSeasons Profile => m_ProfileSeasons;

	[Inject] SeasonsCollection   m_SeasonsCollection;
	[Inject] SeasonsDescriptors  m_SeasonsDescriptors;
	[Inject] ProfileSeasons      m_ProfileSeasons;
	[Inject] ProductsManager m_ProductsManager;

	public void SubscribePass(string _SeasonID, Action _Action) => m_ProductsManager.Profile.SubscribePurchase(_SeasonID, _Action);

	public void UnsubscribePass(string _SeasonID, Action _Action) => m_ProductsManager.Profile.SubscribePurchase(_SeasonID, _Action);

	public async Task<bool> Activate()
	{
		if (Activated)
			return true;
		
		int frame = Time.frameCount;
		
		await Task.WhenAll(
			m_SeasonsCollection.Load(),
			m_SeasonsDescriptors.Load(),
			m_ProfileSeasons.Load()
		);
		
		await Complete();
		
		Activated = true;
		
		return frame == Time.frameCount;
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

	public async Task<RequestState> Collect(string _SeasonID, string _ItemID)
	{
		if (!IsItemAvailable(_SeasonID, _ItemID))
			return RequestState.Fail;
		
		SeasonCollectRequest request = new SeasonCollectRequest(_SeasonID, _ItemID);
		
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

	public bool HasPass(string _SeasonID) => m_ProductsManager.ContainsProduct(_SeasonID);

	public string GetTitle(string _SeasonID) => m_SeasonsDescriptors.GetTitle(_SeasonID);

	public string GetFreeItemID(string _SeasonID, int _Level)
	{
		SeasonLevel level = GetLevel(_SeasonID, _Level);
		
		return level?.FreeItem?.ID;
	}

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

	public string GetPaidItemID(string _SeasonID, int _Level)
	{
		SeasonLevel level = GetLevel(_SeasonID, _Level);
		
		return level?.PaidItem?.ID;
	}

	public long GetCoins(string _SeasonID, string _ItemID)
	{
		SeasonItem item = GetItem(_SeasonID, _ItemID);
		
		return item?.Coins ?? 0;
	}

	public string GetVoucherID(string _SeasonID, string _ItemID)
	{
		SeasonItem item = GetItem(_SeasonID, _ItemID);
		
		return item?.VoucherID ?? string.Empty;
	}

	public string GetSongID(string _SeasonID, string _ItemID)
	{
		SeasonItem item = GetItem(_SeasonID, _ItemID);
		
		return item?.SongID ?? string.Empty;
	}

	public string GetChestID(string _SeasonID, string _ItemID)
	{
		SeasonItem item = GetItem(_SeasonID, _ItemID);
		
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
		
		SeasonLevel level = snapshot.Levels
			.Where(_Level => _Level != null)
			.Aggregate((_A, _B) => _A.Level >= _B.Level && _A.Points <= points ? _A : _B);
		
		return level?.Level ?? 0;
	}

	public float GetProgress(string _SeasonID, int _Level)
	{
		SeasonLevel source = GetSourceLevel(_SeasonID);
		SeasonLevel target = GetTargetLevel(_SeasonID);
		
		int sourceLevel = source?.Level ?? 0;
		int targetLevel = target?.Level ?? 0;
		
		if (_Level > targetLevel)
			return 1;
		
		if (_Level < sourceLevel)
			return 0;
		
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

	public bool IsItemAvailable(string _SeasonID, string _ItemID)
	{
		ProfileSeason snapshot = Profile.GetSnapshot(_SeasonID);
		
		if (snapshot?.ItemIDs != null && snapshot.ItemIDs.Contains(_ItemID))
			return false;
		
		int sourceLevel = GetLevel(_SeasonID);
		int targetLevel = GetItemLevel(_SeasonID, _ItemID);
		
		return sourceLevel >= targetLevel;
	}

	SeasonLevel GetSourceLevel(string _SeasonID)
	{
		SeasonSnapshot snapshot = Collection.GetSnapshot(_SeasonID);
		
		if (snapshot?.Levels == null || snapshot.Levels.Count == 0)
			return null;
		
		long points = GetPoints(_SeasonID);
		
		return snapshot.Levels
			.Where(_Level => _Level != null)
			.Aggregate((_A, _B) => _A.Level >= _B.Level && _A.Points <= points ? _A : _B);
	}

	SeasonLevel GetTargetLevel(string _SeasonID)
	{
		SeasonSnapshot snapshot = Collection.GetSnapshot(_SeasonID);
		
		if (snapshot?.Levels == null || snapshot.Levels.Count == 0)
			return null;
		
		long points = GetPoints(_SeasonID);
		
		return snapshot.Levels
			.Where(_Level => _Level != null)
			.Aggregate((_A, _B) => _A.Level <= _B.Level && _A.Points > points ? _A : _B);
	}

	SeasonLevel GetLevel(string _SeasonID, int _Level)
	{
		SeasonSnapshot snapshot = Collection.GetSnapshot(_SeasonID);
		
		if (snapshot?.Levels == null || snapshot.Levels.Count == 0)
			return null;
		
		return snapshot.Levels
			.Where(_Entry => _Entry != null)
			.FirstOrDefault(_Entry => _Entry.Level == _Level);
	}

	SeasonItem GetItem(string _SeasonID, string _ItemID)
	{
		SeasonSnapshot snapshot = Collection.GetSnapshot(_SeasonID);
		
		if (snapshot?.Levels == null || snapshot.Levels.Count == 0)
			return null;
		
		SeasonItem freeItem = snapshot.Levels
			.Where(_Level => _Level != null)
			.Select(_Level => _Level.FreeItem)
			.Where(_Item => _Item != null)
			.FirstOrDefault(_Item => _Item.ID == _ItemID);
		
		SeasonItem paidItem = snapshot.Levels
			.Where(_Level => _Level != null)
			.Select(_Level => _Level.PaidItem)
			.Where(_Item => _Item != null)
			.FirstOrDefault(_Item => _Item.ID == _ItemID);
		
		return freeItem ?? paidItem;
	}

	int GetItemLevel(string _SeasonID, string _ItemID)
	{
		SeasonSnapshot snapshot = Collection.GetSnapshot(_SeasonID);
		
		if (snapshot?.Levels == null || snapshot.Levels.Count == 0)
			return 0;
		
		int freeLevel = snapshot.Levels
			.Where(_Level => _Level != null)
			.Where(_Level => _Level.FreeItem != null)
			.Where(_Level => _Level.FreeItem.ID == _ItemID)
			.Select(_Level => _Level.Level)
			.DefaultIfEmpty(int.MaxValue)
			.Min();
		
		int paidLevel = snapshot.Levels
			.Where(_Level => _Level != null)
			.Where(_Level => _Level.PaidItem != null)
			.Where(_Level => _Level.PaidItem.ID == _ItemID)
			.Select(_Level => _Level.Level)
			.DefaultIfEmpty(int.MaxValue)
			.Min();
		
		return Mathf.Min(freeLevel, paidLevel);
	}

	public bool IsFreeItem(string _SeasonID, string _ItemID)
	{
		SeasonSnapshot snapshot = Collection.GetSnapshot(_SeasonID);
		
		if (snapshot?.Levels == null || snapshot.Levels.Count == 0)
			return false;
		
		return snapshot.Levels
			.Where(_Level => _Level != null)
			.Select(_Level => _Level.FreeItem)
			.Where(_Item => _Item != null)
			.Any(_Item => _Item.ID == _ItemID);
	}

	public bool IsPaidItem(string _SeasonID, string _ItemID)
	{
		SeasonSnapshot snapshot = Collection.GetSnapshot(_SeasonID);
		
		if (snapshot == null)
			return false;
		
		return snapshot.Levels.Select(_Level => _Level.PaidItem)
			.Where(_Item => _Item != null)
			.Any(_Item => _Item.ID == _ItemID);
	}
}
