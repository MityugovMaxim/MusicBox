using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
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
	const string TAP_GREAT_MULTIPLIER   = "tap_great_multiplier";
	const string TAP_GOOD_MULTIPLIER    = "tap_good_multiplier";
	const string TAP_BAD_MULTIPLIER     = "tap_bad_multiplier";

	const string DOUBLE_PERFECT_MULTIPLIER = "double_perfect_multiplier";
	const string DOUBLE_GREAT_MULTIPLIER   = "double_great_multiplier";
	const string DOUBLE_GOOD_MULTIPLIER    = "double_good_multiplier";
	const string DOUBLE_BAD_MULTIPLIER     = "double_bad_multiplier";

	const string HOLD_PERFECT_MULTIPLIER = "hold_perfect_multiplier";
	const string HOLD_GREAT_MULTIPLIER   = "hold_great_multiplier";
	const string HOLD_GOOD_MULTIPLIER    = "hold_good_multiplier";
	const string HOLD_BAD_MULTIPLIER     = "hold_bad_multiplier";

	const string SCORE_PERFECT_THRESHOLD = "score_perfect_threshold";
	const string SCORE_GREAT_THRESHOLD   = "score_great_threshold";
	const string SCORE_GOOD_THRESHOLD    = "score_good_threshold";

	const string ADS_COOLDOWN = "ads_cooldown";

	const string INPUT_EXTEND = "input_extend";
	const string INPUT_OFFSET = "input_offset";

	const string TUTORIAL_TAP_THRESHOLD    = "tutorial_tap_threshold";
	const string TUTORIAL_DOUBLE_THRESHOLD = "tutorial_double_threshold";
	const string TUTORIAL_HOLD_THRESHOLD   = "tutorial_hold_threshold";
	const string TUTORIAL_BEND_THRESHOLD   = "tutorial_bend_threshold";

	const string SONG_LIBRARY_GROUP_SIZE = "song_library_group_size";
	const string REVIEW_COUNT            = "review_count";
	const string REVIEW_RANK             = "review_rank";
	const string REVIEW_COOLDOWN         = "review_cooldown";

	const string BLUETOOTH_LATENCY = "bluetooth_latency";

	public int   ComboX2                 => GetInt(COMBO_X2);
	public int   ComboX4                 => GetInt(COMBO_X4);
	public int   ComboX6                 => GetInt(COMBO_X6);
	public int   ComboX8                 => GetInt(COMBO_X8);
	public float SongRatio               => GetFloat(SONG_RATIO);
	public float SongIFrames             => GetFloat(SONG_IFRAMES);
	public float TapPerfectMultiplier    => GetFloat(TAP_PERFECT_MULTIPLIER);
	public float TapGreatMultiplier      => GetFloat(TAP_GREAT_MULTIPLIER);
	public float TapGoodMultiplier       => GetFloat(TAP_GOOD_MULTIPLIER);
	public float TapBadMultiplier        => GetFloat(TAP_BAD_MULTIPLIER);
	public float DoublePerfectMultiplier => GetFloat(DOUBLE_PERFECT_MULTIPLIER);
	public float DoubleGreatMultiplier   => GetFloat(DOUBLE_GREAT_MULTIPLIER);
	public float DoubleGoodMultiplier    => GetFloat(DOUBLE_GOOD_MULTIPLIER);
	public float DoubleBadMultiplier     => GetFloat(DOUBLE_BAD_MULTIPLIER);
	public float HoldPerfectMultiplier   => GetFloat(HOLD_PERFECT_MULTIPLIER);
	public float HoldGreatMultiplier     => GetFloat(HOLD_GREAT_MULTIPLIER);
	public float HoldGoodMultiplier      => GetFloat(HOLD_GOOD_MULTIPLIER);
	public float HoldBadMultiplier       => GetFloat(HOLD_BAD_MULTIPLIER);
	public float ScorePerfectThreshold   => GetFloat(SCORE_PERFECT_THRESHOLD);
	public float ScoreGreatThreshold     => GetFloat(SCORE_GREAT_THRESHOLD);
	public float ScoreGoodThreshold      => GetFloat(SCORE_GOOD_THRESHOLD);
	public float AdsCooldown             => GetFloat(ADS_COOLDOWN);
	public float InputExtend             => GetFloat(INPUT_EXTEND);
	public float InputOffset             => GetFloat(INPUT_OFFSET);
	public float TutorialTapThreshold    => GetFloat(TUTORIAL_TAP_THRESHOLD);
	public float TutorialDoubleThreshold => GetFloat(TUTORIAL_DOUBLE_THRESHOLD);
	public float TutorialHoldThreshold   => GetFloat(TUTORIAL_HOLD_THRESHOLD);
	public float TutorialBendThreshold   => GetFloat(TUTORIAL_BEND_THRESHOLD);
	public int   SongLibraryGroupSize    => GetInt(SONG_LIBRARY_GROUP_SIZE);
	public int   ReviewCount             => GetInt(REVIEW_COUNT);
	public int   ReviewRank              => GetInt(REVIEW_RANK);
	public long  ReviewCooldown          => GetLong(REVIEW_COOLDOWN);
	public int   BluetoothLatency        => GetInt(BLUETOOTH_LATENCY);

	readonly Dictionary<string, object> m_DefaultValues = new Dictionary<string, object>()
	{
		// Ads
		{ ADS_COOLDOWN, 40 },
		
		// Combo
		{ COMBO_X2, 10 },
		{ COMBO_X4, 25 },
		{ COMBO_X6, 45 },
		{ COMBO_X8, 70 },
		
		// Game
		{ SONG_RATIO, 0.75f },
		{ SONG_IFRAMES, 0.8f },
		
		// Score
		{ SCORE_GOOD_THRESHOLD, 0.4f },
		{ SCORE_GREAT_THRESHOLD, 0.6f },
		{ SCORE_PERFECT_THRESHOLD, 0.8f },
		
		// Input
		{ INPUT_EXTEND, 30 },
		{ INPUT_OFFSET, 5 },
		
		// Tap
		{ TAP_BAD_MULTIPLIER, 80 },
		{ TAP_GOOD_MULTIPLIER, 180 },
		{ TAP_GREAT_MULTIPLIER, 250 },
		{ TAP_PERFECT_MULTIPLIER, 300 },
		
		// Double
		{ DOUBLE_BAD_MULTIPLIER, 250 },
		{ DOUBLE_GOOD_MULTIPLIER, 600 },
		{ DOUBLE_GREAT_MULTIPLIER, 850 },
		{ DOUBLE_PERFECT_MULTIPLIER, 1000 },
		
		// Hold
		{ HOLD_BAD_MULTIPLIER, 200 },
		{ HOLD_GOOD_MULTIPLIER, 550 },
		{ HOLD_GREAT_MULTIPLIER, 700 },
		{ HOLD_PERFECT_MULTIPLIER, 800 },
		
		// Tutorial
		{ TUTORIAL_TAP_THRESHOLD, 0.6f },
		{ TUTORIAL_DOUBLE_THRESHOLD, 0.6f },
		{ TUTORIAL_HOLD_THRESHOLD, 0.6f },
		{ TUTORIAL_BEND_THRESHOLD, 0.6f },
		
		// Menu
		{ SONG_LIBRARY_GROUP_SIZE, 4 },
		{ REVIEW_COUNT, 2 },
		{ REVIEW_RANK, 3 },
		{ REVIEW_COOLDOWN, 259200000 },
		
		// Settings
		{ BLUETOOTH_LATENCY, 300 },
	};

	public async Task Load()
	{
		foreach (string key in m_DefaultValues.Keys.ToArray())
			m_DefaultValues[key] = Convert.ToString(m_DefaultValues[key], CultureInfo.InvariantCulture);
		
		await FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(m_DefaultValues);
		
		try
		{
			await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		
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