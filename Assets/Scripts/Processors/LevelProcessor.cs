using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class LevelDataUpdateSignal { }

public class LevelSnapshot
{
	public string     ID             { get; }
	public int        Level          { get; }
	public string     Title          { get; }
	public string     Artist         { get; }
	public LevelMode  Mode           { get; }
	public LevelBadge Badge          { get; }
	public float      Length         { get; }
	public float      BPM            { get; }
	public float      Speed          { get; }
	public float      Invincibility  { get; }
	public long       DefaultPayout  { get; }
	public long       BronzePayout   { get; }
	public long       SilverPayout   { get; }
	public long       GoldPayout     { get; }
	public long       PlatinumPayout { get; }
	public long       Price          { get; }
	public long       RevivePrice    { get; }
	public string     Skin           { get; }

	public LevelSnapshot(DataSnapshot _Data)
	{
		ID             = _Data.Key;
		Level          = _Data.GetInt("level");
		Title          = _Data.GetString("title", string.Empty);
		Artist         = _Data.GetString("artist", string.Empty);
		Mode           = _Data.GetEnum<LevelMode>("mode");
		Badge          = _Data.GetEnum<LevelBadge>("badge");
		Length         = _Data.GetFloat("length");
		BPM            = _Data.GetFloat("bpm");
		Speed          = _Data.GetFloat("speed");
		Invincibility  = _Data.GetFloat("invincibility", 0.75f);
		DefaultPayout  = _Data.GetLong("default_payout");
		BronzePayout   = _Data.GetLong("bronze_payout");
		SilverPayout   = _Data.GetLong("silver_payout");
		GoldPayout     = _Data.GetLong("gold_payout");
		PlatinumPayout = _Data.GetLong("platinum_payout");
		Price          = _Data.GetLong("price");
		RevivePrice    = _Data.GetLong("revive_price");
		Skin           = _Data.GetString("skin", "level");
	}
}

[Preserve]
public class LevelProcessor
{
	public bool Playing => m_Level != null && m_Level.Playing;

	bool Loaded { get; set; }

	Level  m_Level;
	string m_LevelID;

	readonly SignalBus        m_SignalBus;
	readonly ProfileProcessor m_ProfileProcessor;
	readonly StorageProcessor m_StorageProcessor;
	readonly Level.Factory    m_LevelFactory;

	readonly List<string>                      m_LevelIDs        = new List<string>();
	readonly Dictionary<string, LevelSnapshot> m_LevelSnapshots  = new Dictionary<string, LevelSnapshot>();
	readonly List<ISampleReceiver>             m_SampleReceivers = new List<ISampleReceiver>();

	DatabaseReference m_LevelsData;

	[Inject]
	public LevelProcessor(
		SignalBus        _SignalBus,
		ProfileProcessor _ProfileProcessor,
		StorageProcessor _StorageProcessor,
		Level.Factory    _LevelFactory
	)
	{
		m_SignalBus        = _SignalBus;
		m_ProfileProcessor = _ProfileProcessor;
		m_StorageProcessor = _StorageProcessor;
		m_LevelFactory     = _LevelFactory;
	}

	public async Task LoadLevels()
	{
		if (m_LevelsData == null)
		{
			m_LevelsData              =  FirebaseDatabase.DefaultInstance.RootReference.Child("levels");
			m_LevelsData.ValueChanged += OnLevelsUpdate;
		}
		
		await FetchLevels();
		
		Loaded = true;
	}

	public List<string> GetLevelIDs()
	{
		return m_LevelIDs.ToList();
	}

	public bool HasLevelID(string _LevelID)
	{
		return m_LevelIDs.Contains(_LevelID);
	}

	public string GetArtist(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get artist failed. Level snapshot with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		return levelSnapshot.Artist;
	}

	public string GetTitle(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get title failed. Level snapshot with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		return levelSnapshot.Title;
	}

	public long GetPayout(string _LevelID, ScoreRank _Rank)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get payout failed. Level snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		long payout = 0;
		if (_Rank >= ScoreRank.None)
			payout += levelSnapshot.DefaultPayout;
		if (_Rank >= ScoreRank.Bronze)
			payout += levelSnapshot.BronzePayout;
		if (_Rank >= ScoreRank.Silver)
			payout += levelSnapshot.SilverPayout;
		if (_Rank >= ScoreRank.Gold)
			payout += levelSnapshot.GoldPayout;
		if (_Rank >= ScoreRank.Platinum)
			payout += levelSnapshot.PlatinumPayout;
		
		return payout;
	}

	public long GetPrice(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get price failed. Level snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return levelSnapshot.Price;
	}

	public long GetRevivePrice(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get revive price failed. Level snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return levelSnapshot.RevivePrice;
	}

	public float GetInvincibility(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get invincibility failed. Level snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return levelSnapshot.Invincibility;
	}

	public int GetLevel(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get level failed. Level snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return levelSnapshot.Level;
	}

	public LevelMode GetMode(string _LevelID)
	{
		if (m_ProfileProcessor.HasNoAds())
			return LevelMode.Free;
		
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get mode failed. Level info with ID '{0}' is null.", _LevelID);
			return LevelMode.Free;
		}
		
		return levelSnapshot.Mode;
	}

	public LevelBadge GetBadge(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get badge failed. Level info with ID '{0}' is null.", _LevelID);
			return LevelBadge.None;
		}
		
		return levelSnapshot.Badge;
	}

	public async Task Load(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogError("[LevelProvider] Create level failed. Level info is null.");
			return;
		}
		
		if (m_Level != null)
		{
			Debug.LogErrorFormat("[LevelProvider] Create level failed. Level instance '{0}' already created.", m_Level.name);
			return;
		}
		
		Track[] tracks = await m_StorageProcessor.LoadTracks(_LevelID);
		
		Level prefab = Resources.Load<Level>(levelSnapshot.Skin);
		
		m_LevelID = _LevelID;
		m_Level   = m_LevelFactory.Create(prefab);
		
		m_Level.Setup(
			levelSnapshot.Length,
			levelSnapshot.BPM,
			levelSnapshot.Speed,
			tracks
		);
		
		m_Level.RegisterSampleReceivers(m_SampleReceivers.ToArray());
		
		m_SignalBus.Fire(new LevelStartSignal(m_LevelID));
	}

	public void Remove()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Remove level failed. Level is null.");
			return;
		}
		
		m_SignalBus.Fire(new LevelExitSignal(m_LevelID));
		
		GameObject.Destroy(m_Level.gameObject);
		
		m_Level   = null;
		m_LevelID = null;
	}

	public void Play()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Play level failed. Level is null.");
			return;
		}
		
		m_Level.Play(() => m_SignalBus.Fire(new LevelFinishSignal(m_LevelID)));
		
		m_SignalBus.Fire(new LevelPlaySignal(m_LevelID));
	}

	public void Pause()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Pause level failed. Level is null.");
			return;
		}
		
		m_Level.Pause();
	}

	public void Restart()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Restart level failed. Level is null.");
			return;
		}
		
		m_Level.Stop();
		
		m_SignalBus.Fire(new LevelRestartSignal(m_LevelID));
	}

	public void AddSampleReceiver(ISampleReceiver _SampleReceiver)
	{
		m_SampleReceivers.Add(_SampleReceiver);
	}

	public void RemoveSampleReceiver(ISampleReceiver _SampleReceiver)
	{
		m_SampleReceivers.Remove(_SampleReceiver);
	}

	async void OnLevelsUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[LevelProcessor] Updating levels data...");
		
		await FetchLevels();
		
		Debug.Log("[LevelProcessor] Update levels data complete.");
		
		m_SignalBus.Fire<LevelDataUpdateSignal>();
	}

	async Task FetchLevels()
	{
		m_LevelIDs.Clear();
		m_LevelSnapshots.Clear();
		
		DataSnapshot levelSnapshots = await m_LevelsData.OrderByChild("order").GetValueAsync(15000, 2);
		
		if (levelSnapshots == null)
		{
			Debug.LogError("[LevelProcessor] Fetch levels failed.");
			return;
		}
		
		foreach (DataSnapshot levelSnapshot in levelSnapshots.Children)
		{
			bool active = levelSnapshot.GetBool("active");
			
			if (!active)
				continue;
			
			LevelSnapshot level = new LevelSnapshot(levelSnapshot);
			
			m_LevelIDs.Add(level.ID);
			m_LevelSnapshots[level.ID] = level;
		}
	}

	LevelSnapshot GetLevelSnapshot(string _LevelID)
	{
		if (string.IsNullOrEmpty(_LevelID))
		{
			Debug.LogError("[ProgressProcessor] Get level snapshot failed. Level ID is null or empty.");
			return null;
		}
		
		if (!m_LevelSnapshots.ContainsKey(_LevelID))
		{
			Debug.LogErrorFormat("[ProgressProcessor] Get level snapshot failed. Level with ID '{0}' not found.", _LevelID);
			return null;
		}
		
		return m_LevelSnapshots[_LevelID];
	}
}