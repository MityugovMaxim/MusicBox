using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Auth;
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
		
		if (m_Snapshots.Count == 0)
			return minLevel;
		
		ProgressSnapshot snapshot = m_Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.Discs <= _Discs)
			.Aggregate((_A, _B) => _A.Level > _B.Level ? _A : _B);
		
		return snapshot != null
			? Mathf.Clamp(snapshot.Level, minLevel, maxLevel)
			: minLevel;
	}

	public float GetProgress(int _Level, int _Discs)
	{
		int minThreshold = GetDiscs(_Level);
		int maxThreshold = GetDiscs(_Level + 1);
		
		if (_Discs >= maxThreshold)
			return 1;
		
		if (_Discs <= minThreshold)
			return 0;
		
		return Mathf.InverseLerp(minThreshold, maxThreshold, _Discs);
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

	void Unload()
	{
		if (m_Data != null)
		{
			m_Data.ValueChanged -= OnUpdate;
			m_Data              =  null;
		}
		
		Loaded = false;
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		if (FirebaseAuth.DefaultInstance.CurrentUser == null)
		{
			Unload();
			return;
		}
		
		Log.Info(this, "Updating progress data...");
		
		await Fetch();
		
		Log.Info(this, "Update progress data complete.");
		
		m_SignalBus.Fire<ProgressDataUpdateSignal>();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.GetValueAsync(15000, 2);
		
		if (dataSnapshot == null)
		{
			Log.Error(this, "Fetch progress failed.");
			return;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(_Data => new ProgressSnapshot(_Data)));
	}

	ProgressSnapshot GetSnapshot(int _Level)
	{
		if (m_Snapshots.Count == 0)
			return null;
		
		int minLevel = GetMinLevel();
		int maxLevel = GetMaxLevel();
		int level    = Mathf.Clamp(_Level, minLevel, maxLevel);
		
		ProgressSnapshot snapshot = m_Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.Level >= level)
			.Aggregate((_A, _B) => _A.Level < _B.Level ? _A : _B);
		
		if (snapshot == null)
			Log.Error(this, "Get snapshot failed. Snapshot with Level '{0}' is null.");
		
		return snapshot;
	}
}