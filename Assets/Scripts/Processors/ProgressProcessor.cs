using System.Linq;
using AudioBox.Compression;
using AudioBox.Logging;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ProgressSnapshot : Snapshot
{
	public bool   Active    { get; }
	public int    Level     { get; }
	public int    Discs     { get; }
	public long   Coins     { get; }
	public string SongID    { get; }
	public string ChestID   { get; }
	public string VoucherID { get; }
	public string FrameID   { get; }

	public ProgressSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active    = _Data.GetBool("active");
		Level     = _Data.GetInt("level");
		Discs     = _Data.GetInt("discs");
		Coins     = _Data.GetLong("coins");
		SongID    = _Data.GetString("song_id");
		ChestID   = _Data.GetString("chest_id");
		VoucherID = _Data.GetString("voucher_id");
		FrameID   = _Data.GetString("frame_id");
	}
}

[Preserve]
public class ProgressProcessor : DataCollection<ProgressSnapshot>
{
	protected override string Path => "progress";

	public int GetDiscs(int _Level)
	{
		ProgressSnapshot snapshot = GetSnapshot(_Level);
		
		return snapshot?.Discs ?? 0;
	}

	public long GetCoins(int _Level)
	{
		ProgressSnapshot snapshot = GetSnapshot(_Level);
		
		return snapshot?.Coins ?? 0;
	}

	public string GetSongID(int _Level)
	{
		ProgressSnapshot snapshot = GetSnapshot(_Level);
		
		return snapshot?.SongID ?? string.Empty;
	}

	public int GetLevel(int _Discs)
	{
		int minLevel = GetMinLevel();
		int maxLevel = GetMaxLevel();
		
		if (Snapshots.Count == 0)
			return minLevel;
		
		ProgressSnapshot snapshot = Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.GreaterMax(_Snapshot => _Snapshot.Discs, _Discs);
		
		return snapshot != null
			? Mathf.Clamp(snapshot.Level, minLevel, maxLevel)
			: minLevel;
	}

	public int GetMinLevel()
	{
		if (Snapshots.Count == 0)
			return 1;
		
		return Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.Level)
			.DefaultIfEmpty(1)
			.Min();
	}

	public int GetMaxLevel()
	{
		if (Snapshots.Count == 0)
			return 1;
		
		return Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.Level)
			.DefaultIfEmpty(1)
			.Max();
	}

	ProgressSnapshot GetSnapshot(int _Level)
	{
		if (Snapshots.Count == 0)
			return null;
		
		int minLevel = GetMinLevel();
		int maxLevel = GetMaxLevel();
		int level    = Mathf.Clamp(_Level, minLevel, maxLevel);
		
		ProgressSnapshot snapshot = Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.Level >= level)
			.Aggregate((_A, _B) => _A.Level < _B.Level ? _A : _B);
		
		if (snapshot == null)
			Log.Error(this, "Get snapshot failed. Snapshot with Level '{0}' is null.");
		
		return snapshot;
	}
}
