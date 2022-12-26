using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using Zenject;

public enum ChestState
{
	Unavailable = 0,
	Available   = 1,
	Selected    = 2,
}

[Preserve]
public class ChestsManager : IDataManager
{
	public ChestsCollection Collection => m_ChestsCollection;

	[Inject] ChestsCollection m_ChestsCollection;

	public Task<bool> Activate() => GroupTask.ProcessAsync(this, Collection.Load);

	public int GetCapacity(RankType _ChestRank)
	{
		ChestSnapshot snapshot = GetSnapshot(_ChestRank);
		
		return snapshot?.Capacity ?? 0;
	}

	public SortedList<double, ChestItemType> GetItems(RankType _ChestRank)
	{
		ChestSnapshot snapshot = GetSnapshot(_ChestRank);
		
		if (snapshot == null || snapshot.Items == null)
			return null;
		
		double total = 0;
		foreach (ChestItem item in snapshot.Items)
			total += item.Weight;
		
		SortedList<double, ChestItemType> items = new SortedList<double, ChestItemType>();
		
		foreach (ChestItem item in snapshot.Items)
			items.Add(item.Weight / total, item.Type);
		
		return items;
	}

	public long GetBoost(RankType _ChestRank)
	{
		ChestSnapshot snapshot = GetSnapshot(_ChestRank);
		
		return snapshot?.Boost ?? 0;
	}

	public long GetTime(RankType _ChestRank)
	{
		ChestSnapshot snapshot = GetSnapshot(_ChestRank);
		
		return snapshot?.Time ?? 0;
	}

	public RankType GetRank(string _ChestID)
	{
		ChestSnapshot snapshot = Collection.GetSnapshot(_ChestID);
		
		return snapshot?.Rank ?? RankType.None;
	}

	public string GetChestID(RankType _ChestRank)
	{
		ChestSnapshot snapshot = GetSnapshot(_ChestRank);
		
		return snapshot?.ID ?? string.Empty;
	}

	ChestSnapshot GetSnapshot(RankType _ChestRank) => Collection.Snapshots.FirstOrDefault(_Snapshot => _Snapshot != null && _Snapshot.Rank == _ChestRank);
}
