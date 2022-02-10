using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class LevelController
{
	public bool Playing => m_Level != null && m_Level.Playing;

	readonly SignalBus         m_SignalBus;
	readonly LevelProcessor    m_LevelProcessor;
	readonly StorageProcessor  m_StorageProcessor;
	readonly Level.Factory     m_LevelFactory;

	readonly List<ISampleReceiver> m_SampleReceivers = new List<ISampleReceiver>();

	string m_LevelID;
	Level  m_Level;

	[Inject]
	public LevelController(
		SignalBus         _SignalBus,
		LevelProcessor    _LevelProcessor,
		StorageProcessor  _StorageProcessor,
		Level.Factory     _LevelFactory
	)
	{
		m_SignalBus        = _SignalBus;
		m_LevelProcessor   = _LevelProcessor;
		m_StorageProcessor = _StorageProcessor;
		m_LevelFactory     = _LevelFactory;
	}

	public async Task<bool> Load(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		string skin = m_LevelProcessor.GetSkin(m_LevelID);
		
		Track[] tracks = await m_StorageProcessor.LoadTracks(m_LevelID);
		
		if (tracks == null || tracks.Length == 0)
		{
			Debug.LogError("[LevelController] Load failed. Tracks is null or empty.");
			return false;
		}
		
		Level prefab = Resources.Load<Level>(skin);
		
		if (prefab == null)
		{
			Debug.LogError("[LevelController] Load failed. Skin is null or empty.");
			return false;
		}
		
		m_Level = m_LevelFactory.Create(prefab);
		
		m_Level.Setup(
			m_LevelProcessor.GetLength(m_LevelID),
			m_LevelProcessor.GetBPM(m_LevelID),
			m_LevelProcessor.GetSpeed(m_LevelID),
			tracks
		);
		
		m_Level.RegisterSampleReceivers(m_SampleReceivers.ToArray());
		
		m_SignalBus.Fire(new LevelStartSignal(m_LevelID));
		
		return true;
	}

	public void Remove()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelController] Remove level failed. Level is null.");
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
			Debug.LogError("[LevelController] Play level failed. Level is null.");
			return;
		}
		
		m_Level.Play(() => m_SignalBus.Fire(new LevelFinishSignal(m_LevelID)));
		
		m_SignalBus.Fire(new LevelPlaySignal(m_LevelID));
	}

	public void Pause()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelController] Pause level failed. Level is null.");
			return;
		}
		
		m_Level.Pause();
	}

	public void Restart()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelController] Restart level failed. Level is null.");
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
}

[Preserve]
public class LevelDataUpdateSignal { }

public class LevelSnapshot
{
	public string                              ID             { get; }
	public bool                                Active         { get; }
	public int                                 Level          { get; }
	public string                              Title          { get; }
	public string                              Artist         { get; }
	public LevelMode                           Mode           { get; }
	public LevelBadge                          Badge          { get; }
	public float                               Length         { get; }
	public float                               BPM            { get; }
	public float                               Speed          { get; }
	public float                               Invincibility  { get; }
	public long                                DefaultPayout  { get; }
	public long                                BronzePayout   { get; }
	public long                                SilverPayout   { get; }
	public long                                GoldPayout     { get; }
	public long                                PlatinumPayout { get; }
	public long                                Price          { get; }
	public long                                RevivePrice    { get; }
	public string                              Skin           { get; }
	public IReadOnlyDictionary<string, string> Platforms      { get; }

	public LevelSnapshot(DataSnapshot _Data)
	{
		ID             = _Data.Key;
		Active         = _Data.GetBool("active");
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
		Platforms      = _Data.GetStringDictionary("platforms");
	}
}

[Preserve]
public class LevelProcessor
{
	bool Loaded { get; set; }

	readonly SignalBus        m_SignalBus;
	readonly ProfileProcessor m_ProfileProcessor;

	readonly List<LevelSnapshot> m_LevelSnapshots = new List<LevelSnapshot>();

	DatabaseReference m_LevelsData;

	[Inject]
	public LevelProcessor(
		SignalBus        _SignalBus,
		ProfileProcessor _ProfileProcessor
	)
	{
		m_SignalBus        = _SignalBus;
		m_ProfileProcessor = _ProfileProcessor;
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
		return m_LevelSnapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public bool HasLevelID(string _LevelID)
	{
		return m_LevelSnapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Any(_Snapshot => _Snapshot.ID == _LevelID);
	}

	public string GetSkin(string _LevelID)
	{
		LevelSnapshot snapshot = GetLevelSnapshot(_LevelID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get skin failed. Snapshot with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		return snapshot.Skin;
	}

	public string GetArtist(string _LevelID)
	{
		LevelSnapshot snapshot = GetLevelSnapshot(_LevelID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get artist failed. Snapshot with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		return snapshot.Artist;
	}

	public string GetTitle(string _LevelID)
	{
		LevelSnapshot snapshot = GetLevelSnapshot(_LevelID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get title failed. Snapshot with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		return snapshot.Title;
	}

	public long GetPayout(string _LevelID, ScoreRank _Rank)
	{
		LevelSnapshot snapshot = GetLevelSnapshot(_LevelID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get payout failed. Snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		long payout = 0;
		if (_Rank >= ScoreRank.None)
			payout += snapshot.DefaultPayout;
		if (_Rank >= ScoreRank.Bronze)
			payout += snapshot.BronzePayout;
		if (_Rank >= ScoreRank.Silver)
			payout += snapshot.SilverPayout;
		if (_Rank >= ScoreRank.Gold)
			payout += snapshot.GoldPayout;
		if (_Rank >= ScoreRank.Platinum)
			payout += snapshot.PlatinumPayout;
		
		return payout;
	}

	public long GetPrice(string _LevelID)
	{
		LevelSnapshot snapshot = GetLevelSnapshot(_LevelID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get price failed. Snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return snapshot.Price;
	}

	public long GetRevivePrice(string _LevelID)
	{
		LevelSnapshot snapshot = GetLevelSnapshot(_LevelID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get revive price failed. Snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return snapshot.RevivePrice;
	}

	public float GetLength(string _LevelID)
	{
		LevelSnapshot snapshot = GetLevelSnapshot(_LevelID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get length failed. Snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return snapshot.Length;
	}

	public float GetBPM(string _LevelID)
	{
		LevelSnapshot snapshot = GetLevelSnapshot(_LevelID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get BPM failed. Snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return snapshot.BPM;
	}

	public float GetSpeed(string _LevelID)
	{
		LevelSnapshot snapshot = GetLevelSnapshot(_LevelID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get speed failed. Snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return snapshot.Speed;
	}

	public float GetInvincibility(string _LevelID)
	{
		LevelSnapshot snapshot = GetLevelSnapshot(_LevelID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get invincibility failed. Snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return snapshot.Invincibility;
	}

	public int GetLevel(string _LevelID)
	{
		LevelSnapshot snapshot = GetLevelSnapshot(_LevelID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get level failed. Snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return snapshot.Level;
	}

	public LevelMode GetMode(string _LevelID)
	{
		if (m_ProfileProcessor.HasNoAds())
			return LevelMode.Free;
		
		LevelSnapshot snapshot = GetLevelSnapshot(_LevelID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get mode failed. Snapshot with ID '{0}' is null.", _LevelID);
			return LevelMode.Free;
		}
		
		return snapshot.Mode;
	}

	public LevelBadge GetBadge(string _LevelID)
	{
		LevelSnapshot snapshot = GetLevelSnapshot(_LevelID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get badge failed. Snapshot with ID '{0}' is null.", _LevelID);
			return LevelBadge.None;
		}
		
		return snapshot.Badge;
	}

	public string GetPlatformURL(string _LevelID, string _PlatformID)
	{
		LevelSnapshot snapshot = GetLevelSnapshot(_LevelID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get platform URL failed. Snapshot with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		if (!snapshot.Platforms.TryGetValue(_PlatformID, out string url))
		{
			Debug.LogErrorFormat("[LevelProcessor] Get platform URL failed. URL not found. Level: {0} Platform: {1}", _LevelID, _PlatformID);
			return string.Empty;
		}
		
		return url;
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
		m_LevelSnapshots.Clear();
		
		DataSnapshot data = await m_LevelsData.OrderByChild("order").GetValueAsync(15000, 2);
		
		if (data == null)
		{
			Debug.LogError("[LevelProcessor] Fetch levels failed.");
			return;
		}
		
		m_LevelSnapshots.AddRange(data.Children.Select(_Data => new LevelSnapshot(_Data)));
	}

	LevelSnapshot GetLevelSnapshot(string _LevelID)
	{
		if (m_LevelSnapshots == null || m_LevelSnapshots.Count == 0)
			return null;
		
		if (string.IsNullOrEmpty(_LevelID))
		{
			Debug.LogError("[ProgressProcessor] Get level snapshot failed. Level ID is null or empty.");
			return null;
		}
		
		return m_LevelSnapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _LevelID);
	}
}