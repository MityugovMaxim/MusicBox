using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class ProgressSnapshot
{
	public bool         Active  { get; }
	public int          Level   { get; }
	public int          Discs   { get; }
	public long         Coins   { get; }
	public List<string> SongIDs { get; }

	public ProgressSnapshot(DataSnapshot _Data)
	{
		Active  = _Data.GetBool("active");
		Level   = _Data.GetInt("level");
		Discs   = _Data.GetInt("discs");
		Coins   = _Data.GetLong("coins");
		SongIDs = _Data.GetChildKeys("song_ids");
	}
}

[Preserve]
public class ProgressDataUpdateSignal { }

[Preserve]
public class ProgressProcessor
{
	bool Loaded { get; set; }

	readonly SignalBus m_SignalBus;

	readonly List<ProgressSnapshot> m_Snapshots = new List<ProgressSnapshot>();

	DatabaseReference m_Data;

	[Inject]
	public ProgressProcessor(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	public async Task Load()
	{
		if (m_Data == null)
		{
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child("progress");
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		Loaded = true;
	}

	public int GetSongLevel(string _SongID)
	{
		if (m_Snapshots.Count == 0)
			return 1;
		
		ProgressSnapshot[] snapshots = m_Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.SongIDs != null && _Snapshot.SongIDs.Count > 0)
			.Where(_Snapshot => _Snapshot.SongIDs.Contains(_SongID))
			.ToArray();
		
		return snapshots.Length > 0
			? snapshots.Aggregate((_A, _B) => _A.Level < _B.Level ? _A : _B).Level
			: 1;
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
		if (m_Snapshots.Count == 0)
			return 1;
		
		ProgressSnapshot progressSnapshot = m_Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.Discs <= _Discs)
			.Aggregate((_A, _B) => _A.Level > _B.Level ? _A : _B);
		
		return progressSnapshot?.Level ?? 1;
	}

	public int ClampLevel(int _Level)
	{
		int minLevel = GetMinLevel();
		int maxLevel = GetMaxLevel();
		return Mathf.Clamp(_Level, minLevel, maxLevel);
	}

	public int GetMinLevel()
	{
		if (m_Snapshots.Count == 0)
			return 1;
		
		return m_Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.Level)
			.DefaultIfEmpty(1)
			.Min();
	}

	public int GetMaxLevel()
	{
		if (m_Snapshots.Count == 0)
			return 1;
		
		return m_Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.Level)
			.DefaultIfEmpty(1)
			.Max();
	}

	public int GetMinLimit(int _Level)
	{
		if (m_Snapshots.Count == 0)
			return 0;
		
		int level = ClampLevel(_Level);
		
		ProgressSnapshot progressSnapshot = m_Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.Level >= level)
			.Aggregate((_A, _B) => _A.Level < _B.Level ? _A : _B);
		
		return progressSnapshot?.Discs ?? 0;
	}

	public int GetMaxLimit(int _Level)
	{
		if (m_Snapshots.Count == 0)
			return 0;
		
		int level = ClampLevel(_Level + 1);
		
		ProgressSnapshot progressSnapshot = m_Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.Level >= level)
			.Aggregate((_A, _B) => _A.Level < _B.Level ? _A : _B);
		
		return progressSnapshot?.Discs ?? 0;
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[ProgressProcessor] Updating progress data...");
		
		await Fetch();
		
		Debug.Log("[ProgressProcessor] Update progress data complete.");
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot progressSnapshots = await m_Data.GetValueAsync(15000, 2);
		
		if (progressSnapshots == null)
		{
			Debug.LogError("[ProgressProcessor] Fetch progress failed.");
			return;
		}
		
		foreach (DataSnapshot progressSnapshot in progressSnapshots.Children)
			m_Snapshots.Add(new ProgressSnapshot(progressSnapshot));
		
		m_SignalBus.Fire<ProgressDataUpdateSignal>();
	}

	ProgressSnapshot GetSnapshot(int _Level)
	{
		if (m_Snapshots.Count == 0)
			return null;
		
		ProgressSnapshot snapshot = m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.Level == _Level);
		
		if (snapshot == null)
			Log.Error(this, "Get snapshot failed. Snapshot with Level '{0}' is null.");
		
		return snapshot;
	}
}