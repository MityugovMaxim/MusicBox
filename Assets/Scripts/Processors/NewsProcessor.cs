using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.RemoteConfig;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ConfigProcessor
{
	const string COMBO_X2                  = "combo_x2";
	const string COMBO_X4                  = "combo_x4";
	const string COMBO_X6                  = "combo_x6";
	const string COMBO_X8                  = "combo_x8";
	const string SONG_RATIO                = "song_ratio";
	const string SONG_IFRAMES              = "song_iframes";

	const string TAP_PERFECT_MULTIPLIER    = "tap_perfect_multiplier";
	const string TAP_GOOD_MULTIPLIER       = "tap_good_multiplier";
	const string TAP_BAD_MULTIPLIER        = "tap_bad_multiplier";

	const string DOUBLE_PERFECT_MULTIPLIER = "double_perfect_multiplier";
	const string DOUBLE_GOOD_MULTIPLIER    = "double_good_multiplier";
	const string DOUBLE_BAD_MULTIPLIER     = "double_bad_multiplier";

	const string HOLD_PERFECT_MULTIPLIER   = "hold_perfect_multiplier";
	const string HOLD_GOOD_MULTIPLIER      = "hold_good_multiplier";
	const string HOLD_BAD_MULTIPLIER       = "hold_bad_multiplier";
	const string HOLD_HIT_MULTIPLIER       = "hold_hit_multiplier";

	const string SCORE_PERFECT_THRESHOLD = "score_perfect_threshold";
	const string SCORE_GOOD_THRESHOLD    = "score_good_threshold";

	const string SONG_RESTART_ADS_COUNT = "song_restart_ads_count";
	const string SONG_LEAVE_ADS_COUNT   = "song_leave_ads_count";
	const string SONG_NEXT_ADS_COUNT    = "song_next_ads_count";

	const string REVIEW_REQUEST_COUNT   = "review_request_count";

	public int   ComboX2                 => GetInt(COMBO_X2);
	public int   ComboX4                 => GetInt(COMBO_X4);
	public int   ComboX6                 => GetInt(COMBO_X6);
	public int   ComboX8                 => GetInt(COMBO_X8);
	public float SongRatio               => GetFloat(SONG_RATIO);
	public float SongIFrames             => GetFloat(SONG_IFRAMES);
	public float TapPerfectMultiplier    => GetFloat(TAP_PERFECT_MULTIPLIER);
	public float TapGoodMultiplier       => GetFloat(TAP_GOOD_MULTIPLIER);
	public float TapBadMultiplier        => GetFloat(TAP_BAD_MULTIPLIER);
	public float DoublePerfectMultiplier => GetFloat(DOUBLE_PERFECT_MULTIPLIER);
	public float DoubleGoodMultiplier    => GetFloat(DOUBLE_GOOD_MULTIPLIER);
	public float DoubleBadMultiplier     => GetFloat(DOUBLE_BAD_MULTIPLIER);
	public float HoldPerfectMultiplier   => GetFloat(HOLD_PERFECT_MULTIPLIER);
	public float HoldGoodMultiplier      => GetFloat(HOLD_GOOD_MULTIPLIER);
	public float HoldBadMultiplier       => GetFloat(HOLD_BAD_MULTIPLIER);
	public float HoldHitMultiplier       => GetFloat(HOLD_HIT_MULTIPLIER);
	public float ScorePerfectThreshold   => GetFloat(SCORE_PERFECT_THRESHOLD);
	public float ScoreGoodThreshold      => GetFloat(SCORE_GOOD_THRESHOLD);
	public int   SongRestartAdsCount     => GetInt(SONG_RESTART_ADS_COUNT);
	public int   SongLeaveAdsCount       => GetInt(SONG_LEAVE_ADS_COUNT);
	public int   SongNextAdsCount        => GetInt(SONG_NEXT_ADS_COUNT);
	public int   ReviewRequestCount      => GetInt(REVIEW_REQUEST_COUNT);

	static readonly Dictionary<string, object> m_DefaultValues = new Dictionary<string, object>()
	{
		// Game
		{ COMBO_X2, 10 },
		{ COMBO_X4, 30 },
		{ COMBO_X6, 90 },
		{ COMBO_X8, 120 },
		{ SONG_RATIO, 0.75f },
		{ SONG_IFRAMES, 0.75f },
		
		// Tap
		{ TAP_PERFECT_MULTIPLIER, 400 },
		{ TAP_GOOD_MULTIPLIER, 200 },
		{ TAP_BAD_MULTIPLIER, 100 },
		
		// Double
		{ DOUBLE_PERFECT_MULTIPLIER, 1000 },
		{ DOUBLE_GOOD_MULTIPLIER, 500 },
		{ DOUBLE_BAD_MULTIPLIER, 100 },
		
		// Hold
		{ HOLD_PERFECT_MULTIPLIER, 1600 },
		{ HOLD_GOOD_MULTIPLIER, 800 },
		{ HOLD_BAD_MULTIPLIER, 100 },
		{ HOLD_HIT_MULTIPLIER, 10 },
		
		// Score
		{ SCORE_PERFECT_THRESHOLD, 0.9f },
		{ SCORE_GOOD_THRESHOLD, 0.4f },
		
		// Ads
		{ SONG_RESTART_ADS_COUNT, 2 },
		{ SONG_LEAVE_ADS_COUNT, 3 },
		{ SONG_NEXT_ADS_COUNT, 2 },
	};

	public async Task Load()
	{
		await FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(m_DefaultValues);
		
		await FirebaseRemoteConfig.DefaultInstance.FetchAsync();
	}

	static int GetInt(string _Key) => (int)GetLong(_Key);

	static float GetFloat(string _Key) => (float)GetDouble(_Key);

	static bool GetBool(string _Key) => GetValue(_Key).BooleanValue;

	static long GetLong(string _Key) => GetValue(_Key).LongValue;

	static double GetDouble(string _Key) => GetValue(_Key).DoubleValue;

	static string GetString(string _Key) => GetValue(_Key).StringValue;

	static ConfigValue GetValue(string _Key) => FirebaseRemoteConfig.DefaultInstance.GetValue(_Key);
}

public class NewsSnapshot
{
	public string ID        { get; }
	public bool   Active    { get; }
	public string Image     { get; }
	public long   Timestamp { get; }
	public string URL       { get; }
	public int    Order     { get; }

	public NewsSnapshot(DataSnapshot _Data)
	{
		ID        = _Data.Key;
		Active    = _Data.GetBool("active");
		Image     = _Data.GetString("image");
		Timestamp = _Data.GetLong("timestamp");
		URL       = _Data.GetString("url");
		Order     = _Data.GetInt("order");
	}
}

[Preserve]
public class NewsDataUpdateSignal { }

[Preserve]
public class NewsDescriptor : DescriptorProcessor<NewsDataUpdateSignal>
{
	protected override string Path => "news_descriptors";
}

[Preserve]
public class NewsProcessor
{
	bool Loaded { get; set; }

	[Inject] SignalBus      m_SignalBus;
	[Inject] NewsDescriptor m_NewsDescriptor;

	readonly List<NewsSnapshot> m_Snapshots = new List<NewsSnapshot>();

	DatabaseReference m_Data;

	public async Task Load()
	{
		if (m_Data == null)
		{
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child("news");
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		await m_NewsDescriptor.Load();
		
		Loaded = true;
	}

	public List<string> GetNewsIDs()
	{
		return m_Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.OrderByDescending(_Snapshot => _Snapshot.Timestamp)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetImage(string _NewsID)
	{
		NewsSnapshot snapshot = GetSnapshot(_NewsID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public string GetTitle(string _NewsID) => m_NewsDescriptor.GetTitle(_NewsID);

	public string GetDescription(string _NewsID) => m_NewsDescriptor.GetDescription(_NewsID);

	public string GetDate(string _NewsID)
	{
		NewsSnapshot snapshot = GetSnapshot(_NewsID);
		
		if (snapshot == null || snapshot.Timestamp == 0)
			return string.Empty;
		
		DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(snapshot.Timestamp);
		
		return date.LocalDateTime.ToShortDateString();
	}

	public string GetURL(string _NewsID)
	{
		NewsSnapshot snapshot = GetSnapshot(_NewsID);
		
		return snapshot?.URL ?? string.Empty;
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[NewsProcessor] Updating news data...");
		
		await Fetch();
		
		Debug.Log("[NewsProcessor] Update news data complete.");
		
		m_SignalBus.Fire<NewsDataUpdateSignal>();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.OrderByChild("order").GetValueAsync(15000, 2);
		
		if (dataSnapshot == null)
		{
			Debug.LogError("[NewsProcessor] Fetch news failed.");
			return;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(_Data => new NewsSnapshot(_Data)));
	}

	NewsSnapshot GetSnapshot(string _NewsID)
	{
		if (m_Snapshots.Count == 0)
			return null;
		
		if (string.IsNullOrEmpty(_NewsID))
		{
			Debug.LogError("[NewsProcessor] Get snapshot failed. News ID is null or empty.");
			return null;
		}
		
		NewsSnapshot snapshot = m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _NewsID);
		
		if (snapshot == null)
			Debug.LogErrorFormat("[NewsProcessor] Get snapshot failed. Snapshot with ID '{0}' is null.", _NewsID);
		
		return snapshot;
	}
}
