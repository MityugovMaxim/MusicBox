using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class SongsDataUpdateSignal { }

public class SongSnapshot
{
	[ClipboardProperty]
	public string ID { get;                                             set; }
	public bool                                Active            { get; set; }
	public string                              Title             { get; set; }
	public string                              Artist            { get; set; }
	public SongMode                            Mode              { get; set; }
	public SongBadge                           Badge             { get; set; }
	public float                               BPM               { get; set; }
	public float                               Speed             { get; set; }
	public long                                DefaultPayout     { get; set; }
	public long                                BronzePayout      { get; set; }
	public long                                SilverPayout      { get; set; }
	public long                                GoldPayout        { get; set; }
	public long                                PlatinumPayout    { get; set; }
	public long                                Price             { get; set; }
	public string                              Skin              { get; set; }
	public int                                 BronzeThreshold   { get; set; }
	public int                                 SilverThreshold   { get; set; }
	public int                                 GoldThreshold     { get; set; }
	public int                                 PlatinumThreshold { get; set; }
	public IReadOnlyDictionary<string, string> Platforms         { get; }
	[HideProperty]
	public int                                 Order             { get; set; }

	public SongSnapshot(string _SongID)
	{
		ID        = _SongID;
		Platforms = new Dictionary<string, string>();
	}

	public SongSnapshot(DataSnapshot _Data)
	{
		ID                = _Data.Key;
		Active            = _Data.GetBool("active");
		Title             = _Data.GetString("title", string.Empty);
		Artist            = _Data.GetString("artist", string.Empty);
		Mode              = _Data.GetEnum<SongMode>("mode");
		Badge             = _Data.GetEnum<SongBadge>("badge");
		BPM               = _Data.GetFloat("bpm");
		Speed             = _Data.GetFloat("speed");
		DefaultPayout     = _Data.GetLong("default_payout");
		BronzePayout      = _Data.GetLong("bronze_payout");
		SilverPayout      = _Data.GetLong("silver_payout");
		GoldPayout        = _Data.GetLong("gold_payout");
		PlatinumPayout    = _Data.GetLong("platinum_payout");
		Price             = _Data.GetLong("price");
		Platforms         = _Data.GetStringDictionary("platforms");
		Skin              = _Data.GetString("skin", "default");
		BronzeThreshold   = _Data.GetInt("bronze_threshold");
		SilverThreshold   = _Data.GetInt("silver_threshold");
		GoldThreshold     = _Data.GetInt("gold_threshold");
		PlatinumThreshold = _Data.GetInt("platinum_threshold");
		Order             = _Data.GetInt("order");
	}

	public Dictionary<string, object> Serialize()
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		data["active"]             = Active;
		data["title"]              = Title;
		data["artist"]             = Artist;
		data["mode"]               = (int)Mode;
		data["badge"]              = (int)Badge;
		data["bpm"]                = BPM;
		data["speed"]              = Speed;
		data["default_payout"]     = DefaultPayout;
		data["bronze_payout"]      = BronzePayout;
		data["silver_payout"]      = SilverPayout;
		data["gold_payout"]        = GoldPayout;
		data["platinum_payout"]    = PlatinumPayout;
		data["price"]              = Price;
		data["platforms"]          = Platforms;
		data["skin"]               = Skin;
		data["bronze_threshold"]   = BronzeThreshold;
		data["silver_threshold"]   = SilverThreshold;
		data["gold_threshold"]     = GoldThreshold;
		data["platinum_threshold"] = PlatinumThreshold;
		data["order"]              = Order;
		
		return data;
	}
}

[Preserve]
public class SongsProcessor
{
	bool Loaded { get; set; }

	[Inject] SignalBus        m_SignalBus;
	[Inject] ProfileProcessor m_ProfileProcessor;

	readonly List<SongSnapshot> m_Snapshots = new List<SongSnapshot>();

	DatabaseReference m_Data;

	public async Task Load()
	{
		if (m_Data == null)
		{
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child("songs");
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		Loaded = true;
	}

	public List<string> GetSongIDs()
	{
		return m_Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetSkin(string _LevelID)
	{
		SongSnapshot snapshot = GetSnapshot(_LevelID);
		
		return snapshot?.Skin ?? "default";
	}

	public string GetArtist(string _LevelID)
	{
		SongSnapshot snapshot = GetSnapshot(_LevelID);
		
		return snapshot?.Artist ?? string.Empty;
	}

	public string GetTitle(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Title ?? string.Empty;
	}

	public long GetPayout(string _SongID, ScoreRank _SourceRank, ScoreRank _TargetRank)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		if (snapshot == null)
			return 0;
		
		long GetRankPayout(ScoreRank _Rank)
		{
			switch (_Rank)
			{
				case ScoreRank.Bronze:
					return snapshot.BronzePayout;
				case ScoreRank.Silver:
					return snapshot.SilverPayout;
				case ScoreRank.Gold:
					return snapshot.GoldPayout;
				case ScoreRank.Platinum:
					return snapshot.PlatinumPayout;
				default:
					return 0;
			}
		}
		
		long payout = 0;
		foreach (ScoreRank rank in Enum.GetValues(typeof(ScoreRank)))
		{
			if (rank <= _SourceRank)
				payout += GetRankPayout(rank);
		}
		
		for (ScoreRank rank = _SourceRank; rank <= _TargetRank; rank++)
		{
			if (rank > _SourceRank)
				payout += GetRankPayout(rank);
		}
		
		return payout;
	}

	public long GetPrice(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Price ?? 0;
	}

	public string GetAppleMusicURL(string _SongID)
	{
		const string platformID = "apple_music";
		
		return GetPlatformURL(_SongID, platformID);
	}

	public string GetSpotifyURL(string _SongID)
	{
		const string platformID = "spotify";
		
		return GetPlatformURL(_SongID, platformID);
	}

	public string GetDeezerURL(string _SongID)
	{
		const string platformID = "deezer";
		
		return GetPlatformURL(_SongID, platformID);
	}

	bool ContainsPlatformURL(string _SongID, string _PlatformID)
	{
		if (string.IsNullOrEmpty(_PlatformID))
			return false;
		
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot != null && snapshot.Platforms != null && snapshot.Platforms.ContainsKey(_PlatformID);
	}

	string GetPlatformURL(string _SongID, string _PlatformID)
	{
		if (!ContainsPlatformURL(_SongID, _PlatformID))
			return string.Empty;
		
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot.Platforms[_PlatformID];
	}

	public float GetBPM(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.BPM ?? 0;
	}

	public float GetSpeed(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Speed ?? 0;
	}

	public ScoreRank GetRank(string _SongID, int _Accuracy)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		if (snapshot == null)
			return ScoreRank.None;
		
		if (_Accuracy >= snapshot.PlatinumThreshold)
			return ScoreRank.Platinum;
		else if (_Accuracy >= snapshot.GoldThreshold)
			return ScoreRank.Gold;
		else if (_Accuracy >= snapshot.SilverThreshold)
			return ScoreRank.Silver;
		else if (_Accuracy >= snapshot.BronzeThreshold)
			return ScoreRank.Bronze;
		else
			return ScoreRank.None;
	}

	public int GetThreshold(string _SongID, ScoreRank _Rank)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		if (snapshot == null)
			return 0;
		
		switch (_Rank)
		{
			case ScoreRank.Platinum:
				return snapshot.PlatinumThreshold;
			
			case ScoreRank.Gold:
				return snapshot.GoldThreshold;
			
			case ScoreRank.Silver:
				return snapshot.SilverThreshold;
			
			case ScoreRank.Bronze:
				return snapshot.BronzeThreshold;
			
			default:
				return 0;
		}
	}

	public SongMode GetMode(string _SongID)
	{
		if (m_ProfileProcessor.HasNoAds())
			return SongMode.Free;
		
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Mode ?? SongMode.Free;
	}

	public SongBadge GetBadge(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Badge ?? SongBadge.None;
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[SongsProcessor] Updating songs data...");
		
		await Fetch();
		
		Debug.Log("[SongsProcessor] Update songs data complete.");
		
		m_SignalBus.Fire<SongsDataUpdateSignal>();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.OrderByChild("order").GetValueAsync(15000, 2);
		
		if (dataSnapshot == null)
		{
			Debug.LogError("[SongsProcessor] Fetch songs failed.");
			return;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(_Data => new SongSnapshot(_Data)));
	}

	public async Task Upload()
	{
		Loaded = false;
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (SongSnapshot snapshot in m_Snapshots)
		{
			if (snapshot != null)
				data[snapshot.ID] = snapshot.Serialize();
		}
		
		await m_Data.SetValueAsync(data);
		
		await Fetch();
		
		Loaded = true;
		
		m_SignalBus.Fire<SongsDataUpdateSignal>();
	}

	public async Task Upload(params string[] _SongIDs)
	{
		if (_SongIDs == null || _SongIDs.Length == 0)
			return;
		
		Loaded = false;
		
		foreach (string songID in _SongIDs.Distinct())
		{
			SongSnapshot snapshot = GetSnapshot(songID);
			
			Dictionary<string, object> data = snapshot?.Serialize();
			
			await m_Data.Child(songID).SetValueAsync(data);
		}
		
		await Fetch();
		
		Loaded = true;
		
		m_SignalBus.Fire<SongsDataUpdateSignal>();
	}

	public void MoveSnapshot(string _SongID, int _Offset)
	{
		int sourceIndex = m_Snapshots.FindIndex(_Snapshot => _Snapshot.ID == _SongID);
		int targetIndex = sourceIndex + _Offset;
		
		if (sourceIndex < 0 || sourceIndex >= m_Snapshots.Count || targetIndex < 0 || targetIndex >= m_Snapshots.Count)
			return;
		
		(m_Snapshots[sourceIndex], m_Snapshots[targetIndex]) = (m_Snapshots[targetIndex], m_Snapshots[sourceIndex]);
		
		for (int i = 0; i < m_Snapshots.Count; i++)
			m_Snapshots[i].Order = i;
		
		m_SignalBus.Fire<SongsDataUpdateSignal>();
	}

	public SongSnapshot CreateSnapshot()
	{
		DatabaseReference reference = m_Data.Push();
		
		string songID = reference.Key;
		
		SongSnapshot snapshot = new SongSnapshot(songID);
		
		m_Snapshots.Insert(0, snapshot);
		
		return snapshot;
	}

	public void RemoveSnapshot(string _SongID)
	{
		m_Snapshots.RemoveAll(_Snapshot => _Snapshot.ID == _SongID);
		
		m_SignalBus.Fire<SongsDataUpdateSignal>();
	}

	public SongSnapshot GetSnapshot(string _SongID)
	{
		if (m_Snapshots.Count == 0)
			return null;
		
		if (string.IsNullOrEmpty(_SongID))
		{
			Debug.LogError("[SongsProcessor] Get snapshot failed. Song ID is null or empty.");
			return null;
		}
		
		SongSnapshot snapshot = m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _SongID);
		
		if (snapshot == null)
			Debug.LogErrorFormat("[SongsProcessor] Get snapshot failed. Snapshot with ID '{0}' is null.", _SongID);
		
		return snapshot;
	}
}