using System.Collections.Generic;
using System.Linq;
using AudioBox.Logging;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ProgressSnapshot : Snapshot
{
	public bool         Active  { get; }
	public int          Level   { get; }
	public int          Discs   { get; }
	public long         Coins   { get; }
	public List<string> SongIDs { get; }

	public ProgressSnapshot() : base("new_progress", 0)
	{
		Active  = false;
		Level   = 1;
		Discs   = 0;
		Coins   = 0;
		SongIDs = new List<string>();
	}

	public ProgressSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active  = _Data.GetBool("active");
		Level   = _Data.GetInt("level");
		Discs   = _Data.GetInt("discs");
		Coins   = _Data.GetLong("coins");
		SongIDs = _Data.GetChildKeys("song_ids");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["active"]   = Active;
		_Data["level"]    = Level;
		_Data["discs"]    = Discs;
		_Data["coins"]    = Coins;
		_Data["song_ids"] = SongIDs.ToDictionary(_SongID => _SongID, _SongID => true);
	}
}

[Preserve]
public class ProgressDataUpdateSignal { }

[Preserve]
public class ProgressProcessor : DataProcessor<ProgressSnapshot, ProgressDataUpdateSignal>
{
	protected override string Path => "progress";

	public int GetSongLevel(string _SongID)
	{
		if (Snapshots.Count == 0)
			return 1;
		
		ProgressSnapshot[] snapshots = Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.SongIDs != null && _Snapshot.SongIDs.Count > 0)
			.Where(_Snapshot => _Snapshot.SongIDs.Contains(_SongID))
			.ToArray();
		
		return snapshots.Length > 0
			? snapshots.Aggregate((_A, _B) => _A.Level < _B.Level ? _A : _B).Level
			: 1;
	}

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

	public List<string> GetSongIDs(int _Level)
	{
		ProgressSnapshot snapshot = GetSnapshot(_Level);
		
		if (snapshot == null || snapshot.SongIDs == null)
			return new List<string>();
		
		return snapshot.SongIDs;
	}

	public int GetLevel(int _Discs)
	{
		int minLevel = GetMinLevel();
		int maxLevel = GetMaxLevel();
		
		if (Snapshots.Count == 0)
			return minLevel;
		
		ProgressSnapshot snapshot = Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.Discs <= _Discs)
			.Aggregate((_A, _B) => _A.Level > _B.Level ? _A : _B);
		
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