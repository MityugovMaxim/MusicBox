using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using Zenject;

public class ProgressSnapshot
{
	public int Level    { get; }
	public int MinLimit { get; }
	public int MaxLimit { get; }

	public ProgressSnapshot(DataSnapshot _Data)
	{
		Level    = _Data.GetInt("level");
		MinLimit = _Data.GetInt("min_limit");
		MaxLimit = _Data.GetInt("max_limit");
	}
}

public class ProgressDataUpdateSignal { }

public class ProgressProcessor
{
	public bool Loaded { get; private set; }

	readonly SignalBus m_SignalBus;

	readonly List<ProgressSnapshot> m_ProgressSnapshots = new List<ProgressSnapshot>();

	DatabaseReference m_ProgressData;

	[Inject]
	public ProgressProcessor(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	public async Task LoadProgress()
	{
		if (m_ProgressData == null)
			m_ProgressData = FirebaseDatabase.DefaultInstance.RootReference.Child("progress");
		
		await FetchProgress();
		
		if (Loaded)
			return;
		
		Loaded = true;
		
		m_ProgressData.ValueChanged += OnProgressUpdate;
	}

	public int GetLevel(int _Discs)
	{
		if (m_ProgressSnapshots.Count == 0)
			return 1;
		
		ProgressSnapshot progressSnapshot = m_ProgressSnapshots
			.Where(_Snapshot => _Snapshot.MinLimit <= _Discs)
			.Aggregate((_A, _B) => _A.MaxLimit > _B.MaxLimit ? _A : _B);
		
		return progressSnapshot?.Level ?? 1;
	}

	public float GetProgress(int _Discs)
	{
		if (m_ProgressSnapshots.Count == 0)
			return 0;
		
		ProgressSnapshot progressSnapshot = m_ProgressSnapshots
			.Where(_Snapshot => _Snapshot.MinLimit <= _Discs)
			.Aggregate((_A, _B) => _A.MaxLimit > _B.MaxLimit ? _A : _B);
		
		int minDiscs = progressSnapshot?.MinLimit ?? 0;
		int maxDiscs = progressSnapshot?.MaxLimit ?? 0;
		
		return Mathf.InverseLerp(minDiscs, maxDiscs, _Discs);
	}

	public int GetMinLevel()
	{
		if (m_ProgressSnapshots.Count == 0)
			return 1;
		
		return m_ProgressSnapshots.Min(_Snapshot => _Snapshot.Level);
	}

	public int GetMaxLevel()
	{
		if (m_ProgressSnapshots.Count == 0)
			return 1;
		
		return m_ProgressSnapshots.Max(_Snapshot => _Snapshot.Level);
	}

	public int GetMinLimit(int _Level)
	{
		if (m_ProgressSnapshots.Count == 0)
			return 0;
		
		int level = Mathf.Clamp(_Level, GetMinLevel(), GetMaxLevel());
		
		ProgressSnapshot progressSnapshot = m_ProgressSnapshots
			.Where(_Snapshot => _Snapshot.Level >= level)
			.Aggregate((_A, _B) => _A.Level < _B.Level ? _A : _B);
		
		return progressSnapshot?.MinLimit ?? 0;
	}

	public int GetMaxLimit(int _Level)
	{
		if (m_ProgressSnapshots.Count == 0)
			return 0;
		
		int level = Mathf.Clamp(_Level, GetMinLevel(), GetMaxLevel());
		
		ProgressSnapshot progressSnapshot = m_ProgressSnapshots
			.Where(_Snapshot => _Snapshot.Level >= level)
			.Aggregate((_A, _B) => _A.Level < _B.Level ? _A : _B);
		
		return progressSnapshot?.MaxLimit ?? 0;
	}

	async void OnProgressUpdate(object _Sender, EventArgs _Args)
	{
		Debug.Log("[ProgressProcessor] Updating progress data...");
		
		await FetchProgress();
		
		Debug.Log("[ProgressProcessor] Update progress data complete.");
	}

	async Task FetchProgress()
	{
		m_ProgressSnapshots.Clear();
		
		DataSnapshot progressSnapshots = await m_ProgressData.GetValueAsync();
		
		foreach (DataSnapshot progressSnapshot in progressSnapshots.Children)
			m_ProgressSnapshots.Add(new ProgressSnapshot(progressSnapshot));
		
		m_SignalBus.Fire<ProgressDataUpdateSignal>();
	}
}