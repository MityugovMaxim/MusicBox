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
	public string                              ID                { get; }
	public bool                                Active            { get; }
	public int                                 Level             { get; }
	public string                              Title             { get; }
	public string                              Artist            { get; }
	public SongMode                           Mode              { get; }
	public SongBadge                           Badge             { get; }
	public float                               BPM               { get; }
	public float                               Speed             { get; }
	public float                               Ratio             { get; }
	public float                               Invincibility     { get; }
	public long                                DefaultPayout     { get; }
	public long                                BronzePayout      { get; }
	public long                                SilverPayout      { get; }
	public long                                GoldPayout        { get; }
	public long                                PlatinumPayout    { get; }
	public long                                Price             { get; }
	public string                              Skin              { get; }
	public int                                 BronzeThreshold   { get; }
	public int                                 SilverThreshold   { get; }
	public int                                 GoldThreshold     { get; }
	public int                                 PlatinumThreshold { get; }
	public IReadOnlyDictionary<string, string> Platforms         { get; }
	public int                                 Order             { get; }

	public SongSnapshot(DataSnapshot _Data)
	{
		ID                = _Data.Key;
		Active            = _Data.GetBool("active");
		Level             = _Data.GetInt("level");
		Title             = _Data.GetString("title", string.Empty);
		Artist            = _Data.GetString("artist", string.Empty);
		Mode              = _Data.GetEnum<SongMode>("mode");
		Badge             = _Data.GetEnum<SongBadge>("badge");
		Ratio             = _Data.GetFloat("ratio", 0.75f);
		BPM               = _Data.GetFloat("bpm");
		Speed             = _Data.GetFloat("speed");
		Invincibility     = _Data.GetFloat("invincibility", 0.75f);
		DefaultPayout     = _Data.GetLong("default_payout");
		BronzePayout      = _Data.GetLong("bronze_payout");
		SilverPayout      = _Data.GetLong("silver_payout");
		GoldPayout        = _Data.GetLong("gold_payout");
		PlatinumPayout    = _Data.GetLong("platinum_payout");
		Price             = _Data.GetLong("price");
		Platforms         = _Data.GetStringDictionary("platforms");
		Skin              = _Data.GetString("skin", "default");
		BronzeThreshold   = _Data.GetInt("bronze_threshold", 5);
		SilverThreshold   = _Data.GetInt("silver_threshold", 40);
		GoldThreshold     = _Data.GetInt("gold_threshold", 90);
		PlatinumThreshold = _Data.GetInt("platinum_threshold", 95);
		Order             = _Data.GetInt("order");
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
			.Where(_Snapshot => _Snapshot.Active)
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

	public float GetRatio(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Ratio ?? 0;
	}

	public float GetSpeed(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Speed ?? 0;
	}

	public float GetInvincibility(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Invincibility ?? 0;
	}

	public int GetLevel(string _SongID)
	{
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Level ?? 0;
	}

	public int GetThreshold(string _SongID, ScoreRank _Rank)
	{
		if (_Rank < ScoreRank.None)
			return 0;
		
		const int maxThreshold = 100;
		
		SongSnapshot snapshot = GetSnapshot(_SongID);
		
		if (snapshot == null)
			return maxThreshold;
		
		switch (_Rank)
		{
			case ScoreRank.None:
				return 0;
			case ScoreRank.Bronze:
				return snapshot.BronzeThreshold;
			case ScoreRank.Silver:
				return snapshot.SilverThreshold;
			case ScoreRank.Gold:
				return snapshot.GoldThreshold;
			case ScoreRank.Platinum:
				return snapshot.PlatinumThreshold;
			default:
				return maxThreshold;
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

	SongSnapshot GetSnapshot(string _SongID)
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