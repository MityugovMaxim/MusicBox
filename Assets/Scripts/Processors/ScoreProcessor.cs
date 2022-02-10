using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public enum ScoreRank
{
	None     = 0,
	Bronze   = 1,
	Silver   = 2,
	Gold     = 3,
	Platinum = 4,
}

public class ScoreSnapshot
{
	public string    LevelID  { get; }
	public int       Accuracy { get; }
	public long      Score    { get; }
	public ScoreRank Rank     { get; }

	public ScoreSnapshot(DataSnapshot _Data)
	{
		LevelID  = _Data.Key;
		Accuracy = _Data.GetInt("accuracy");
		Score    = _Data.GetLong("score");
		Rank     = _Data.GetEnum<ScoreRank>("rank");
	}
}

public class ScoreDataUpdateSignal { }

[Preserve]
public class ScoreProcessor : IInitializable, IDisposable
{
	const float HOLD_SUCCESS_MULTIPLIER   = 50;
	const float HOLD_FAIL_MULTIPLIER      = 30;
	const float HOLD_HIT_MULTIPLIER       = 5;
	const float HOLD_MISS_MULTIPLIER      = -10;
	const float TAP_PERFECT_MULTIPLIER    = 10;
	const float TAP_GOOD_MULTIPLIER       = 6;
	const float TAP_BAD_MULTIPLIER        = 4;
	const float DOUBLE_PERFECT_MULTIPLIER = 20;
	const float DOUBLE_GOOD_MULTIPLIER    = 12;
	const float DOUBLE_BAD_MULTIPLIER     = 8;
	const float TAP_PERFECT_THRESHOLD     = 0.65f;
	const float TAP_GOOD_THRESHOLD        = 0.35f;
	const float DOUBLE_PERFECT_THRESHOLD  = 0.65f;
	const float DOUBLE_GOOD_THRESHOLD     = 0.35f;

	const int PLATINUM_RANK = 98;
	const int GOLD_RANK     = 85;
	const int SILVER_RANK   = 50;
	const int BRONZE_RANK   = 5;

	const int X8_COMBO = 320;
	const int X6_COMBO = 150;
	const int X4_COMBO = 60;
	const int X2_COMBO = 20;

	bool Loaded { get; set; }

	public long Score => m_Score;

	public int Accuracy
	{
		get
		{
			float accuracy = (float)(SourceAccuracyScore / TargetAccuracyScore);
			
			return Mathf.RoundToInt(Mathf.Clamp01(accuracy) * 100);
		}
	}

	public ScoreRank Rank
	{
		get
		{
			int accuracy = Accuracy;
			if (accuracy >= PLATINUM_RANK)
				return ScoreRank.Platinum;
			else if (accuracy > GOLD_RANK)
				return ScoreRank.Gold;
			else if (accuracy > SILVER_RANK)
				return ScoreRank.Silver;
			else if (accuracy > BRONZE_RANK)
				return ScoreRank.Bronze;
			else
				return ScoreRank.None;
		}
	}

	public int Combo
	{
		get => m_Combo;
		set
		{
			if (m_Combo == value)
				return;
			
			m_Combo = value;
			
			m_SignalBus.Fire(new LevelComboSignal(m_Combo, Multiplier, Progress));
		}
	}

	public int Multiplier
	{
		get
		{
			if (Combo >= X8_COMBO)
				return 8;
			else if (Combo >= X6_COMBO)
				return 6;
			else if (Combo >= X4_COMBO)
				return 4;
			else if (Combo >= X2_COMBO)
				return 2;
			else
				return 1;
		}
	}

	public float Progress
	{
		get
		{
			int minProgress;
			if (Combo >= X8_COMBO)
				minProgress = X8_COMBO;
			else if (Combo >= X6_COMBO)
				minProgress = X6_COMBO;
			else if (Combo >= X4_COMBO)
				minProgress = X4_COMBO;
			else if (Combo >= X2_COMBO)
				minProgress = X2_COMBO;
			else
				minProgress = 0;
			
			int maxProgress;
			if (Combo >= X8_COMBO)
				maxProgress = X8_COMBO;
			else if (Combo >= X6_COMBO)
				maxProgress = X8_COMBO;
			else if (Combo >= X4_COMBO)
				maxProgress = X6_COMBO;
			else if (Combo >= X2_COMBO)
				maxProgress = X4_COMBO;
			else
				maxProgress = X2_COMBO;
			
			return Mathf.InverseLerp(minProgress, maxProgress - 1, Combo);
		}
	}

	double SourceAccuracyScore
	{
		get
		{
			double score = 0;
			
			double holdScore = 0;
			holdScore += m_HoldSuccess * HOLD_SUCCESS_MULTIPLIER;
			holdScore += m_HoldFail * HOLD_FAIL_MULTIPLIER;
			holdScore += m_HoldHit * HOLD_HIT_MULTIPLIER;
			holdScore += m_HoldMiss * HOLD_MISS_MULTIPLIER;
			holdScore *= m_HoldSuccessScore + m_HoldFailScore;
			score     += (long)holdScore;
			
			double tapScore = 0;
			tapScore += m_TapPerfect * TAP_PERFECT_MULTIPLIER;
			tapScore += m_TapGood * TAP_GOOD_MULTIPLIER;
			tapScore += m_TapBad * TAP_BAD_MULTIPLIER;
			score    += (long)tapScore;
			
			double doubleScore = 0;
			doubleScore += m_DoublePerfect * DOUBLE_PERFECT_MULTIPLIER;
			doubleScore += m_DoubleGood * DOUBLE_GOOD_MULTIPLIER;
			doubleScore += m_DoubleBad * DOUBLE_BAD_MULTIPLIER;
			score       += (long)doubleScore;
			
			return (long)score;
		}
	}

	double TargetAccuracyScore
	{
		get
		{
			const double holdCoefficient = 0.98;
			
			double score = 0;
			
			double holdCount   = m_HoldSuccess + m_HoldFail;
			double tapCount    = m_TapPerfect + m_TapGood + m_TapBad + m_TapFail;
			double doubleCount = m_DoublePerfect + m_DoubleGood + m_DoubleBad + m_DoubleFail;
			
			score += holdCount * HOLD_SUCCESS_MULTIPLIER * holdCoefficient;
			score += holdCount * HOLD_HIT_MULTIPLIER;
			score *= holdCount;
			
			score += tapCount * TAP_PERFECT_MULTIPLIER;
			
			score += doubleCount * DOUBLE_PERFECT_MULTIPLIER;
			
			return score;
		}
	}

	readonly SignalBus       m_SignalBus;
	readonly SocialProcessor m_SocialProcessor;

	readonly List<ScoreSnapshot> m_ScoreSnapshots = new List<ScoreSnapshot>();

	DatabaseReference m_ScoresData;

	long   m_Score;
	int    m_Combo;

	int m_TapPerfect;
	int m_TapGood;
	int m_TapBad;
	int m_TapFail;

	int m_DoublePerfect;
	int m_DoubleGood;
	int m_DoubleBad;
	int m_DoubleFail;

	int   m_HoldSuccess;
	int   m_HoldFail;
	int   m_HoldHit;
	int   m_HoldMiss;
	float m_HoldSuccessScore;
	float m_HoldFailScore;

	[Inject]
	public ScoreProcessor(SignalBus _SignalBus, SocialProcessor _SocialProcessor)
	{
		m_SignalBus       = _SignalBus;
		m_SocialProcessor = _SocialProcessor;
	}

	public async Task LoadScores()
	{
		if (m_ScoresData != null && m_ScoresData.Key != m_SocialProcessor.UserID)
		{
			Debug.LogFormat("[ScoreProcessor] Change user. From: {0} To: {1}.", m_ScoresData.Key, m_SocialProcessor.UserID);
			
			Loaded                    =  false;
			m_ScoresData.ValueChanged -= OnScoresUpdate;
			m_ScoresData              =  null;
		}
		
		if (m_ScoresData == null)
		{
			m_ScoresData              =  FirebaseDatabase.DefaultInstance.RootReference.Child("scores").Child(m_SocialProcessor.UserID);
			m_ScoresData.ValueChanged += OnScoresUpdate;
		}
		
		await FetchScores();
		
		Loaded = true;
	}

	public int GetAccuracy(string _LevelID)
	{
		ScoreSnapshot snapshot = GetScoreSnapshot(_LevelID);
		
		return snapshot?.Accuracy ?? 0;
	}

	public long GetScore(string _LevelID)
	{
		ScoreSnapshot snapshot = GetScoreSnapshot(_LevelID);
		
		return snapshot?.Score ?? 0;
	}

	public ScoreRank GetRank(string _LevelID)
	{
		ScoreSnapshot snapshot = GetScoreSnapshot(_LevelID);
		
		return snapshot?.Rank ?? ScoreRank.None;
	}

	public int GetRankMinAccuracy(ScoreRank _Rank)
	{
		switch (_Rank)
		{
			case ScoreRank.None:
				return 0;
			case ScoreRank.Bronze:
				return BRONZE_RANK;
			case ScoreRank.Silver:
				return SILVER_RANK;
			case ScoreRank.Gold:
				return GOLD_RANK;
			case ScoreRank.Platinum:
				return PLATINUM_RANK;
			default:
				return PLATINUM_RANK;
		}
	}

	public int GetRankMaxAccuracy(ScoreRank _Rank)
	{
		switch (_Rank)
		{
			case ScoreRank.None:
				return BRONZE_RANK;
			case ScoreRank.Bronze:
				return SILVER_RANK;
			case ScoreRank.Silver:
				return GOLD_RANK;
			case ScoreRank.Gold:
				return PLATINUM_RANK;
			case ScoreRank.Platinum:
				return 100;
			default:
				return 100;
		}
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(Restore);
		m_SignalBus.Subscribe<LevelRestartSignal>(Restore);
		
		m_SignalBus.Subscribe<HoldSuccessSignal>(RegisterHoldSuccess);
		m_SignalBus.Subscribe<HoldFailSignal>(RegisterHoldFail);
		m_SignalBus.Subscribe<HoldHitSignal>(RegisterHoldHit);
		m_SignalBus.Subscribe<HoldMissSignal>(RegisterHoldMiss);
		
		m_SignalBus.Subscribe<TapSuccessSignal>(RegisterTapSuccess);
		m_SignalBus.Subscribe<TapFailSignal>(RegisterTapFail);
		
		m_SignalBus.Subscribe<DoubleSuccessSignal>(RegisterDoubleSuccess);
		m_SignalBus.Subscribe<DoubleFailSignal>(RegisterDoubleFail);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(Restore);
		m_SignalBus.Unsubscribe<LevelRestartSignal>(Restore);
		
		m_SignalBus.Unsubscribe<HoldSuccessSignal>(RegisterHoldSuccess);
		m_SignalBus.Unsubscribe<HoldFailSignal>(RegisterHoldFail);
		m_SignalBus.Unsubscribe<HoldHitSignal>(RegisterHoldHit);
		m_SignalBus.Unsubscribe<HoldMissSignal>(RegisterHoldMiss);
		
		m_SignalBus.Unsubscribe<TapSuccessSignal>(RegisterTapSuccess);
		m_SignalBus.Unsubscribe<TapFailSignal>(RegisterTapFail);
		
		m_SignalBus.Unsubscribe<DoubleSuccessSignal>(RegisterDoubleSuccess);
		m_SignalBus.Unsubscribe<DoubleFailSignal>(RegisterDoubleFail);
	}

	async void OnScoresUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded || m_ScoresData.Key != m_SocialProcessor.UserID)
			return;
		
		Debug.Log("[ScoreProcessor] Updating scores data...");
		
		await FetchScores();
		
		Debug.Log("[ScoreProcessor] Update scores data complete.");
		
		m_SignalBus.Fire<ScoreDataUpdateSignal>();
	}

	async Task FetchScores()
	{
		m_ScoreSnapshots.Clear();
		
		DataSnapshot data = await m_ScoresData.GetValueAsync(15000, 2);
		
		if (data == null)
		{
			Debug.LogError("[ScoreProcessor] Fetch scores failed.");
			return;
		}
		
		foreach (DataSnapshot entry in data.Children)
		{
			ScoreSnapshot snapshot = new ScoreSnapshot(entry);
			m_ScoreSnapshots.Add(snapshot);
		}
	}

	void Restore()
	{
		m_Score = 0;
		m_Combo = 0;
		
		m_TapPerfect       = 0;
		m_TapGood          = 0;
		m_TapBad           = 0;
		m_TapFail          = 0;
		m_DoublePerfect    = 0;
		m_DoubleGood       = 0;
		m_DoubleBad        = 0;
		m_DoubleFail       = 0;
		m_HoldSuccess      = 0;
		m_HoldFail         = 0;
		m_HoldHit          = 0;
		m_HoldMiss         = 0;
		m_HoldSuccessScore = 0;
		m_HoldFailScore    = 0;
	}

	void RegisterHoldSuccess(HoldSuccessSignal _Signal)
	{
		Combo++;
		
		float progress = _Signal.Progress;
		
		m_Score += (long)(progress * HOLD_SUCCESS_MULTIPLIER * Multiplier);
		
		m_HoldSuccessScore += progress;
		m_HoldSuccess++;
		
		m_SignalBus.Fire(new LevelScoreSignal(m_Score));
	}

	void RegisterHoldFail(HoldFailSignal _Signal)
	{
		Combo = 0;
		
		float progress = _Signal.Progress;
		
		m_Score += (long)(progress * HOLD_FAIL_MULTIPLIER * Multiplier);
		
		m_HoldFailScore += progress;
		m_HoldFail++;
		
		m_SignalBus.Fire(new LevelScoreSignal(m_Score));
	}

	void RegisterHoldHit(HoldHitSignal _Signal)
	{
		m_HoldHit++;
		
		m_SignalBus.Fire(new LevelScoreSignal(m_Score));
	}

	void RegisterHoldMiss(HoldMissSignal _Signal)
	{
		Combo = 0;
		
		m_HoldMiss++;
		
		m_SignalBus.Fire(new LevelScoreSignal(m_Score));
	}

	void RegisterTapSuccess(TapSuccessSignal _Signal)
	{
		Combo++;
		
		float progress = _Signal.Progress;
		
		if (progress >= TAP_PERFECT_THRESHOLD)
		{
			m_TapPerfect++;
			m_Score += (long)(progress * TAP_PERFECT_MULTIPLIER * Multiplier);
		}
		else if (progress >= TAP_GOOD_THRESHOLD)
		{
			m_TapGood++;
			m_Score += (long)(progress * TAP_GOOD_MULTIPLIER * Multiplier);
		}
		else
		{
			m_TapBad++;
			m_Score += (long)(progress * TAP_BAD_MULTIPLIER * Multiplier);
		}
		
		m_SignalBus.Fire(new LevelScoreSignal(m_Score));
	}

	void RegisterTapFail(TapFailSignal _Signal)
	{
		Combo = 0;
		
		m_TapFail++;
		
		m_SignalBus.Fire(new LevelScoreSignal(m_Score));
	}

	void RegisterDoubleSuccess(DoubleSuccessSignal _Signal)
	{
		Combo++;
		
		float progress = _Signal.Progress * Multiplier;
		
		if (progress >= DOUBLE_PERFECT_THRESHOLD)
		{
			m_DoublePerfect++;
			m_Score += (long)(progress * DOUBLE_PERFECT_MULTIPLIER * Multiplier);
		}
		else if (progress >= DOUBLE_GOOD_THRESHOLD)
		{
			m_DoubleGood++;
			m_Score += (long)(progress * DOUBLE_GOOD_MULTIPLIER * Multiplier);
		}
		else
		{
			m_DoubleBad++;
			m_Score += (long)(progress * DOUBLE_BAD_MULTIPLIER * Multiplier);
		}
		
		m_SignalBus.Fire(new LevelScoreSignal(m_Score));
	}

	void RegisterDoubleFail(DoubleFailSignal _Signal)
	{
		Combo = 0;
		
		m_DoubleFail++;
		
		m_SignalBus.Fire(new LevelScoreSignal(m_Score));
	}

	ScoreSnapshot GetScoreSnapshot(string _LevelID)
	{
		if (m_ScoreSnapshots == null || m_ScoreSnapshots.Count == 0)
			return null;
		
		if (string.IsNullOrEmpty(_LevelID))
		{
			Debug.LogError("[ScoreProcessor] Get score snapshot failed. Level ID is null or empty.");
			return null;
		}
		
		return m_ScoreSnapshots.FirstOrDefault(_Snapshot => _Snapshot.LevelID == _LevelID);
	}
}