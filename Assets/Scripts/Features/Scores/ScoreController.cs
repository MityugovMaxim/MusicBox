using System.Collections.Generic;
using AudioBox.ASF;
using UnityEngine;
using Zenject;

public class ScoreController
{
	public DynamicDelegate<int>   OnMultiplierChange = new DynamicDelegate<int>();
	public DynamicDelegate<float> OnProgressChange   = new DynamicDelegate<float>();
	public DynamicDelegate<long>  OnScoreChange      = new DynamicDelegate<long>();
	public DynamicDelegate<int>   OnComboChange      = new DynamicDelegate<int>();

	public DynamicDelegate<ScoreType>             OnSelect = new DynamicDelegate<ScoreType>();
	public DynamicDelegate<ScoreType, ScoreGrade> OnHit    = new DynamicDelegate<ScoreType, ScoreGrade>();
	public DynamicDelegate                        OnMiss   = new DynamicDelegate();
	public DynamicDelegate<ScoreType>             OnFail   = new DynamicDelegate<ScoreType>();

	public int Multiplier
	{
		get => m_Multiplier;
		set
		{
			if (m_Multiplier == value)
				return;
			
			m_Multiplier = value;
			
			OnMultiplierChange?.Invoke(m_Multiplier);
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
			
			OnComboChange?.Invoke(m_Combo);
		}
	}

	public float Progress
	{
		get => m_Progress;
		set
		{
			if (Mathf.Approximately(m_Progress, value))
				return;
			
			m_Progress = value;
			
			OnProgressChange?.Invoke(value);
		}
	}

	public long Score
	{
		get => m_Score;
		set
		{
			if (m_Score == value)
				return;
			
			m_Score = value;
			
			OnScoreChange?.Invoke(m_Score);
		}
	}

	public int Accuracy => 100;// (int)MathUtility.RemapClamped(Score, 0, TotalScore, 0, 100);

	public long  TotalScore    { get; private set; }
	public long  BronzeScore   { get; private set; }
	public long  SilverScore   { get; private set; }
	public long  GoldScore     { get; private set; }
	public long  PlatinumScore { get; private set; }

	float TapPerfect { get; set; }
	float TapGreat   { get; set; }
	float TapGood    { get; set; }
	float TapBad     { get; set; }

	float DoublePerfect { get; set; }
	float DoubleGreat   { get; set; }
	float DoubleGood    { get; set; }
	float DoubleBad     { get; set; }

	float HoldPerfect { get; set; }
	float HoldGreat   { get; set; }
	float HoldGood    { get; set; }
	float HoldBad     { get; set; }

	int ComboX2 { get; set; }
	int ComboX4 { get; set; }
	int ComboX6 { get; set; }
	int ComboX8 { get; set; }

	float PerfectThreshold { get; set; }
	float GreatThreshold   { get; set; }
	float GoodThreshold    { get; set; }

	[Inject] ConfigProcessor   m_ConfigProcessor;
	[Inject] DifficultyManager m_DifficultyManager;

	readonly Dictionary<ScoreGrade, int> m_Statistics = new Dictionary<ScoreGrade, int>();

	int   m_Combo;
	int   m_Multiplier = 1;
	float m_Progress;
	long  m_Score;

	public void Setup(RankType _SongType, ASFFile _ASF)
	{
		Restore();
		
		PerfectThreshold = m_ConfigProcessor.ScorePerfectThreshold;
		GreatThreshold   = m_ConfigProcessor.ScoreGreatThreshold;
		GoodThreshold    = m_ConfigProcessor.ScoreGoodThreshold;
		
		TapPerfect = m_ConfigProcessor.TapPerfectMultiplier;
		TapGreat   = m_ConfigProcessor.TapGreatMultiplier;
		TapGood    = m_ConfigProcessor.TapGoodMultiplier;
		TapBad     = m_ConfigProcessor.TapBadMultiplier;
		
		DoublePerfect = m_ConfigProcessor.DoublePerfectMultiplier;
		DoubleGreat   = m_ConfigProcessor.DoubleGreatMultiplier;
		DoubleGood    = m_ConfigProcessor.DoubleGoodMultiplier;
		DoubleBad     = m_ConfigProcessor.DoubleBadMultiplier;
		
		HoldPerfect = m_ConfigProcessor.HoldPerfectMultiplier;
		HoldGreat   = m_ConfigProcessor.HoldGreatMultiplier;
		HoldGood    = m_ConfigProcessor.HoldGoodMultiplier;
		HoldBad     = m_ConfigProcessor.HoldBadMultiplier;
		
		ComboX2 = m_ConfigProcessor.ComboX2;
		ComboX4 = m_ConfigProcessor.ComboX4;
		ComboX6 = m_ConfigProcessor.ComboX6;
		ComboX8 = m_ConfigProcessor.ComboX8;
		
		List<ScoreType> types = _ASF.Aggregate(ScoreType.Tap, ScoreType.Double, ScoreType.Hold);
		
		TotalScore = 0;
		
		for (int combo = 0; combo < types.Count; combo++)
			TotalScore += GetScore(types[combo], PerfectThreshold) * GetMultiplier(combo);
		
		int bronzeThreshold   = m_DifficultyManager.GetThreshold(_SongType, RankType.Bronze);
		int silverThreshold   = m_DifficultyManager.GetThreshold(_SongType, RankType.Bronze);
		int goldThreshold     = m_DifficultyManager.GetThreshold(_SongType, RankType.Bronze);
		int platinumThreshold = m_DifficultyManager.GetThreshold(_SongType, RankType.Bronze);
		
		BronzeScore   = TotalScore / 100 * bronzeThreshold;
		SilverScore   = TotalScore / 100 * silverThreshold;
		GoldScore     = TotalScore / 100 * goldThreshold;
		PlatinumScore = TotalScore / 100 * platinumThreshold;
	}

	public long GetScore(ScoreType _ScoreType, float _Progress)
	{
		switch (_ScoreType)
		{
			case ScoreType.Tap:    return GetTapScore(_Progress);
			case ScoreType.Double: return GetDoubleScore(_Progress);
			case ScoreType.Hold:   return GetHoldScore(_Progress);
			default:               return 0;
		}
	}

	public ScoreGrade GetGrade(float _Progress)
	{
		if (_Progress >= PerfectThreshold)
			return ScoreGrade.Perfect;
		if (_Progress >= GreatThreshold)
			return ScoreGrade.Great;
		if (_Progress >= GoodThreshold)
			return ScoreGrade.Good;
		return ScoreGrade.Bad;
	}

	long GetTapScore(float _Progress) => GetScore(_Progress, TapPerfect, TapGreat, TapGood, TapBad);

	long GetDoubleScore(float _Progress) => GetScore(_Progress, DoublePerfect, DoubleGreat, DoubleGood, DoubleBad);

	long GetHoldScore(float _Progress) => GetScore(_Progress, HoldPerfect, HoldGreat, HoldGood, HoldBad);

	long GetScore(float _Progress, float _Perfect, float _Great, float _Good, float _Bad)
	{
		ScoreGrade scoreGrade = GetGrade(_Progress);
		
		switch (scoreGrade)
		{
			case ScoreGrade.Perfect: return (long)(_Perfect * _Progress);
			case ScoreGrade.Great:   return (long)(_Great * _Progress);
			case ScoreGrade.Good:    return (long)(_Good * _Progress);
			case ScoreGrade.Bad:     return (long)(_Bad * _Progress);
			default:                 return 0;
		}
	}

	int GetMultiplier(int _Combo)
	{
		if (_Combo >= ComboX8)
			return 8;
		if (_Combo >= ComboX6)
			return 6;
		if (_Combo >= ComboX4)
			return 4;
		if (_Combo >= ComboX2)
			return 2;
		return 1;
	}

	float GetProgress(int _Combo)
	{
		if (_Combo >= ComboX8)
			return 1;
		if (_Combo >= ComboX6)
			return MathUtility.Remap01(_Combo, ComboX6, ComboX8 - 1);
		if (_Combo >= ComboX4)
			return MathUtility.Remap01(_Combo, ComboX4, ComboX6 - 1);
		if (_Combo >= ComboX2)
			return MathUtility.Remap01(_Combo, ComboX2, ComboX4 - 1);
		return MathUtility.Remap01(_Combo, 0, ComboX2 - 1);
	}

	public ScoreGrade GetGrade(ScoreType _ScoreType, float _Progress)
	{
		return ScoreGrade.None;
	}

	public void Restore()
	{
		Score      = 0;
		Combo      = 0;
		Multiplier = 1;
		Progress   = 0;
		
		m_Statistics.Clear();
	}

	public void TapHit(float _Progress) => Hit(ScoreType.Tap, _Progress);

	public void DoubleHit(float _Progress) => Hit(ScoreType.Double, _Progress);

	public void HoldHit(float _Progress)
	{
		ScoreGrade scoreGrade = GetGrade(_Progress);
		
		if (scoreGrade == ScoreGrade.Bad || scoreGrade == ScoreGrade.Fail || scoreGrade == ScoreGrade.Miss)
			Combo = 0;
		else
			Combo++;
		
		Multiplier = GetMultiplier(Combo);
		Progress   = GetProgress(Combo);
		
		Register(scoreGrade);
		
		OnSelect?.Invoke(ScoreType.Hold);
		OnHit?.Invoke(ScoreType.Hold, scoreGrade);
	}

	public void HoldRelease(float _Progress, float _Length)
	{
		ScoreGrade scoreGrade = GetGrade(_Progress);
		
		Multiplier = GetMultiplier(Combo);
		Progress   = GetProgress(Combo);
		
		Score += (long)(GetScore(ScoreType.Hold, _Progress) * Multiplier * (double)_Length);
		
		OnHit?.Invoke(ScoreType.Hold, scoreGrade);
	}

	public void Miss()
	{
		Combo      = 0;
		Multiplier = GetMultiplier(Combo);
		Progress   = GetProgress(Combo);
		
		Register(ScoreGrade.Miss);
		
		OnMiss?.Invoke();
	}

	public void TapFail() => Fail(ScoreType.Tap);

	public void DoubleFail() => Fail(ScoreType.Double);

	public void HoldFail() => Fail(ScoreType.Hold);

	public RankType GetRank()
	{
		if (Score >= PlatinumScore)
			return RankType.Platinum;
		if (Score >= GoldScore)
			return RankType.Gold;
		if (Score >= SilverScore)
			return RankType.Silver;
		if (Score >= BronzeScore)
			return RankType.Bronze;
		return RankType.None;
	}

	public long GetScore(int _Accuracy)
	{
		return TotalScore / 100 * _Accuracy;
	}

	public int GetStatistics(ScoreGrade _ScoreGrade)
	{
		return m_Statistics.TryGetValue(_ScoreGrade, out int value) ? value : 0;
	}

	void Hit(ScoreType _Type, float _Progress)
	{
		ScoreGrade scoreGrade = GetGrade(_Progress);
		
		if (scoreGrade == ScoreGrade.Bad || scoreGrade == ScoreGrade.Fail || scoreGrade == ScoreGrade.Miss)
			Combo = 0;
		else
			Combo++;
		
		Multiplier = GetMultiplier(Combo);
		Progress   = GetProgress(Combo);
		
		Score += GetScore(_Type, _Progress) * Multiplier;
		
		Register(scoreGrade);
		
		OnSelect?.Invoke(_Type);
		OnHit?.Invoke(_Type, scoreGrade);
	}

	void Fail(ScoreType _Type)
	{
		Combo      = 0;
		Multiplier = GetMultiplier(Combo);
		Progress   = GetProgress(Combo);
		
		Register(ScoreGrade.Fail);
		
		OnFail?.Invoke(_Type);
	}

	void Register(ScoreGrade _ScoreGrade)
	{
		if (m_Statistics.ContainsKey(_ScoreGrade))
			m_Statistics[_ScoreGrade]++;
		else
			m_Statistics[_ScoreGrade] = 1;
	}
}
