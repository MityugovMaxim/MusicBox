using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Firebase.RemoteConfig;
using UnityEngine.Scripting;

[Preserve]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class ConfigProcessor
{
	const string COMBO_X2     = "combo_x2";
	const string COMBO_X4     = "combo_x4";
	const string COMBO_X6     = "combo_x6";
	const string COMBO_X8     = "combo_x8";
	const string SONG_RATIO   = "song_ratio";
	const string SONG_IFRAMES = "song_iframes";

	const string TAP_PERFECT_MULTIPLIER = "tap_perfect_multiplier";
	const string TAP_GOOD_MULTIPLIER    = "tap_good_multiplier";
	const string TAP_BAD_MULTIPLIER     = "tap_bad_multiplier";

	const string DOUBLE_PERFECT_MULTIPLIER = "double_perfect_multiplier";
	const string DOUBLE_GOOD_MULTIPLIER    = "double_good_multiplier";
	const string DOUBLE_BAD_MULTIPLIER     = "double_bad_multiplier";

	const string HOLD_PERFECT_MULTIPLIER = "hold_perfect_multiplier";
	const string HOLD_GOOD_MULTIPLIER    = "hold_good_multiplier";
	const string HOLD_BAD_MULTIPLIER     = "hold_bad_multiplier";
	const string HOLD_HIT_MULTIPLIER     = "hold_hit_multiplier";

	const string SCORE_PERFECT_THRESHOLD = "score_perfect_threshold";
	const string SCORE_GOOD_THRESHOLD    = "score_good_threshold";

	const string SONG_RESTART_ADS_COUNT = "song_restart_ads_count";
	const string SONG_LEAVE_ADS_COUNT   = "song_leave_ads_count";
	const string SONG_NEXT_ADS_COUNT    = "song_next_ads_count";
	const string SONG_PLAY_ADS_COUNT    = "song_play_ads_count";

	const string REVIEW_REQUEST_COUNT = "review_request_count";

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
	public int   SongPlayAdsCount        => GetInt(SONG_PLAY_ADS_COUNT);
	public int   ReviewRequestCount      => GetInt(REVIEW_REQUEST_COUNT);

	readonly Dictionary<string, object> m_DefaultValues = new Dictionary<string, object>()
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
		{ SCORE_PERFECT_THRESHOLD, 0.9 },
		{ SCORE_GOOD_THRESHOLD, 0.4 },
		
		// Ads
		{ SONG_RESTART_ADS_COUNT, 2 },
		{ SONG_LEAVE_ADS_COUNT, 3 },
		{ SONG_NEXT_ADS_COUNT, 2 },
		{ SONG_PLAY_ADS_COUNT, 4 },
		
		{ REVIEW_REQUEST_COUNT, 2 }
	};

	public async Task Load()
	{
		foreach (string key in m_DefaultValues.Keys.ToArray())
			m_DefaultValues[key] = Convert.ToString(m_DefaultValues[key], CultureInfo.InvariantCulture);
		
		await FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(m_DefaultValues);
		
		await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
		
		await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
	}

	static int GetInt(string _Key) => Convert.ToInt32(GetValue(_Key).StringValue, CultureInfo.InvariantCulture);

	static float GetFloat(string _Key) => Convert.ToSingle(GetValue(_Key).StringValue, CultureInfo.InvariantCulture);

	static bool GetBool(string _Key) => GetValue(_Key).BooleanValue;

	static long GetLong(string _Key) => GetValue(_Key).LongValue;

	static double GetDouble(string _Key) => GetValue(_Key).DoubleValue;

	static string GetString(string _Key) => GetValue(_Key).StringValue;

	static ConfigValue GetValue(string _Key) => FirebaseRemoteConfig.DefaultInstance.GetValue(_Key);
}