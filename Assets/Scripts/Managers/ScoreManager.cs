using System;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class ScoreSignal
{
	public long       Score      { get; }
	public int        Combo      { get; }
	public int        Multiplier { get; }
	public float      Progress   { get; }
	public ScoreGrade Grade      { get; }

	public ScoreSignal(long _Score, int _Combo, int _Multiplier, float _Progress, ScoreGrade _Grade)
	{
		Score      = _Score;
		Combo      = _Combo;
		Multiplier = _Multiplier;
		Progress   = _Progress;
		Grade      = _Grade;
	}
}

public enum ScoreGrade
{
	None,
	Perfect,
	Good,
	Bad,
	Fail,
}

[Preserve]
public class ScoreManager : IInitializable, IDisposable
{
	long Score { get; set; }
	int  Combo { get; set; }

	int Multiplier
	{
		get
		{
			if (Combo >= ComboX8)
				return 8;
			else if (Combo >= ComboX6)
				return 6;
			else if (Combo >= ComboX4)
				return 4;
			else if (Combo >= ComboX2)
				return 2;
			else
				return 1;
		}
	}

	float Progress
	{
		get
		{
			int minProgress;
			if (Combo >= ComboX8)
				minProgress = ComboX8;
			else if (Combo >= ComboX6)
				minProgress = ComboX6;
			else if (Combo >= ComboX4)
				minProgress = ComboX4;
			else if (Combo >= ComboX2)
				minProgress = ComboX2;
			else
				minProgress = 0;
			
			int maxProgress;
			if (Combo >= ComboX8)
				maxProgress = ComboX8;
			else if (Combo >= ComboX6)
				maxProgress = ComboX8;
			else if (Combo >= ComboX4)
				maxProgress = ComboX6;
			else if (Combo >= ComboX2)
				maxProgress = ComboX4;
			else
				maxProgress = ComboX2;
			
			return Mathf.InverseLerp(minProgress, maxProgress - 1, Combo);
		}
	}

	int ComboX8 { get; set; }
	int ComboX6 { get; set; }
	int ComboX4 { get; set; }
	int ComboX2 { get; set; }

	float ScorePerfectThreshold { get; set; }
	float ScoreGoodThreshold    { get; set; }

	float TapPerfectMultiplier { get; set; }
	float TapGoodMultiplier    { get; set; }
	float TapBadMultiplier     { get; set; }

	float DoublePerfectMultiplier { get; set; }
	float DoubleGoodMultiplier    { get; set; }
	float DoubleBadMultiplier     { get; set; }

	float HoldPerfectMultiplier { get; set; }
	float HoldGoodMultiplier    { get; set; }
	float HoldBadMultiplier     { get; set; }
	float HoldHitMultiplier     { get; set; }

	[Inject] SignalBus        m_SignalBus;
	[Inject] ScoresProcessor  m_ScoresProcessor;
	[Inject] SongsProcessor   m_SongsProcessor;
	[Inject] ProfileProcessor m_ProfileProcessor;
	[Inject] ConfigProcessor  m_ConfigProcessor;

	string m_SongID;

	int m_Miss;

	int m_TapPerfect;
	int m_TapGood;
	int m_TapBad;
	int m_TapFail;

	int m_DoublePerfect;
	int m_DoubleGood;
	int m_DoubleBad;
	int m_DoubleFail;

	int m_HoldPerfect;
	int m_HoldGood;
	int m_HoldBad;
	int m_HoldFail;

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<InputMissSignal>(RegisterInputMiss);
		
		m_SignalBus.Subscribe<TapSuccessSignal>(RegisterTapSuccess);
		m_SignalBus.Subscribe<TapFailSignal>(RegisterTapFail);
		
		m_SignalBus.Subscribe<DoubleSuccessSignal>(RegisterDoubleSuccess);
		m_SignalBus.Subscribe<DoubleFailSignal>(RegisterDoubleFail);
		
		m_SignalBus.Subscribe<HoldSuccessSignal>(RegisterHoldSuccess);
		m_SignalBus.Subscribe<HoldFailSignal>(RegisterHoldFail);
		m_SignalBus.Subscribe<HoldHitSignal>(RegisterHoldHit);
		m_SignalBus.Subscribe<HoldMissSignal>(RegisterHoldMiss);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<InputMissSignal>(RegisterInputMiss);
		
		m_SignalBus.Unsubscribe<TapSuccessSignal>(RegisterTapSuccess);
		m_SignalBus.Unsubscribe<TapFailSignal>(RegisterTapFail);
		
		m_SignalBus.Unsubscribe<DoubleSuccessSignal>(RegisterDoubleSuccess);
		m_SignalBus.Unsubscribe<DoubleFailSignal>(RegisterDoubleFail);
		
		m_SignalBus.Unsubscribe<HoldSuccessSignal>(RegisterHoldSuccess);
		m_SignalBus.Unsubscribe<HoldFailSignal>(RegisterHoldFail);
		m_SignalBus.Unsubscribe<HoldHitSignal>(RegisterHoldHit);
		m_SignalBus.Unsubscribe<HoldMissSignal>(RegisterHoldMiss);
	}

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		ComboX8 = m_ConfigProcessor.ComboX8;
		ComboX6 = m_ConfigProcessor.ComboX6;
		ComboX4 = m_ConfigProcessor.ComboX4;
		ComboX2 = m_ConfigProcessor.ComboX2;
		
		ScorePerfectThreshold = m_ConfigProcessor.ScorePerfectThreshold;
		ScoreGoodThreshold    = m_ConfigProcessor.ScoreGoodThreshold;
		
		TapPerfectMultiplier = m_ConfigProcessor.TapPerfectMultiplier;
		TapGoodMultiplier    = m_ConfigProcessor.TapGoodMultiplier;
		TapBadMultiplier     = m_ConfigProcessor.TapBadMultiplier;
		
		DoublePerfectMultiplier = m_ConfigProcessor.DoublePerfectMultiplier;
		DoubleGoodMultiplier    = m_ConfigProcessor.DoubleGoodMultiplier;
		DoubleBadMultiplier     = m_ConfigProcessor.DoubleBadMultiplier;
		
		HoldPerfectMultiplier = m_ConfigProcessor.HoldPerfectMultiplier;
		HoldGoodMultiplier    = m_ConfigProcessor.HoldGoodMultiplier;
		HoldBadMultiplier     = m_ConfigProcessor.HoldBadMultiplier;
		HoldHitMultiplier     = m_ConfigProcessor.HoldHitMultiplier;
	}

	public void Restore()
	{
		m_Miss = 0;
		
		m_TapPerfect = 0;
		m_TapGood    = 0;
		m_TapBad     = 0;
		m_TapFail    = 0;
		
		m_DoublePerfect = 0;
		m_DoubleGood    = 0;
		m_DoubleBad     = 0;
		m_DoubleFail    = 0;
		
		m_HoldPerfect = 0;
		m_HoldGood    = 0;
		m_HoldBad     = 0;
		m_HoldFail    = 0;
		
		Score = 0;
		Combo = 0;
		
		ProcessScore();
	}

	public int GetAccuracy()
	{
		float perfectMultiplier = m_ConfigProcessor.AccuracyPerfectMultiplier;
		float goodMultiplier    = m_ConfigProcessor.AccuracyGoodMultiplier;
		float badMultiplier     = m_ConfigProcessor.AccuracyBadMultiplier;
		
		float tapCount    = m_TapPerfect + m_TapGood + m_TapBad + m_TapFail;
		float doubleCount = m_DoublePerfect + m_DoubleGood + m_DoubleBad + m_DoubleFail;
		float holdCount   = m_HoldPerfect + m_HoldGood + m_HoldBad + m_HoldFail;
		
		float accuracy = 0;
		int   count    = 0;
		
		// Tap accuracy
		if (tapCount > 0)
		{
			accuracy += m_TapPerfect / tapCount * perfectMultiplier;
			accuracy += m_TapGood / tapCount * goodMultiplier;
			accuracy += m_TapBad / tapCount * badMultiplier;
			count++;
		}
		
		// Double accuracy
		if (doubleCount > 0)
		{
			accuracy += m_DoublePerfect / doubleCount * perfectMultiplier;
			accuracy += m_DoubleGood / doubleCount * goodMultiplier;
			accuracy += m_DoubleBad / doubleCount * badMultiplier;
			count++;
		}
		
		// Hold accuracy
		if (holdCount > 0)
		{
			accuracy += m_HoldPerfect / holdCount * perfectMultiplier;
			accuracy += m_HoldGood / holdCount * goodMultiplier;
			accuracy += m_HoldBad / holdCount * badMultiplier;
			count++;
		}
		
		accuracy = Mathf.Max(0, accuracy - m_Miss * goodMultiplier);
		
		accuracy = Mathf.Clamp01(accuracy / count);
		
		return Mathf.FloorToInt(accuracy * 100);
	}

	public ScoreRank GetRank()
	{
		int accuracy = GetAccuracy();
		
		return m_SongsProcessor.GetRank(m_SongID, accuracy);
	}

	public long GetScore()
	{
		return Score;
	}

	public int GetSourceDiscs()
	{
		return m_ProfileProcessor.Discs;
	}

	public int GetTargetDiscs()
	{
		ScoreRank sourceRank = m_ScoresProcessor.GetRank(m_SongID);
		ScoreRank targetRank = GetRank();
		
		int discs = Mathf.Max(0, targetRank - sourceRank);
		
		return m_ProfileProcessor.Discs + discs;
	}

	public float GetRankProgress(ScoreRank _Rank, int _Accuracy)
	{
		int minThreshold = m_SongsProcessor.GetThreshold(m_SongID, _Rank);
		int maxThreshold = m_SongsProcessor.GetThreshold(m_SongID, _Rank + 1);
		
		if (_Accuracy <= minThreshold)
			return 0;
		
		if (_Accuracy >= maxThreshold)
			return 1;
		
		return Mathf.InverseLerp(minThreshold, maxThreshold, _Accuracy);
	}

	void RegisterInputMiss()
	{
		Combo = 0;
		
		m_Miss++;
		
		ProcessScore(ScoreGrade.Bad);
	}

	void RegisterTapSuccess(TapSuccessSignal _Signal)
	{
		ScoreGrade grade;
		if (_Signal.Progress >= ScorePerfectThreshold)
		{
			Combo++;
			
			grade = ScoreGrade.Perfect;
			m_TapPerfect++;
			AddScore(_Signal.Progress * TapPerfectMultiplier);
		}
		else if (_Signal.Progress >= ScoreGoodThreshold)
		{
			Combo++;
			
			grade = ScoreGrade.Good;
			m_TapGood++;
			AddScore(_Signal.Progress * TapGoodMultiplier);
		}
		else
		{
			Combo = 0;
			
			grade = ScoreGrade.Bad;
			m_TapBad++;
			AddScore(_Signal.Progress * TapBadMultiplier);
		}
		
		ProcessScore(grade);
	}

	void RegisterTapFail(TapFailSignal _Signal)
	{
		m_TapFail++;
		
		Combo = 0;
		
		ProcessScore(ScoreGrade.Fail);
	}

	void RegisterDoubleSuccess(DoubleSuccessSignal _Signal)
	{
		ScoreGrade grade;
		if (_Signal.Progress >= ScorePerfectThreshold)
		{
			Combo++;
			
			grade = ScoreGrade.Perfect;
			m_DoublePerfect++;
			AddScore(_Signal.Progress * DoublePerfectMultiplier);
		}
		else if (_Signal.Progress >= ScoreGoodThreshold)
		{
			Combo++;
			
			grade = ScoreGrade.Good;
			m_DoubleGood++;
			AddScore(_Signal.Progress * DoubleGoodMultiplier);
		}
		else
		{
			Combo = 0;
			
			grade = ScoreGrade.Bad;
			m_DoubleBad++;
			AddScore(_Signal.Progress * DoubleBadMultiplier);
		}
		
		ProcessScore(grade);
	}

	void RegisterDoubleFail(DoubleFailSignal _Signal)
	{
		m_DoubleFail++;
		
		Combo = 0;
		
		ProcessScore(ScoreGrade.Fail);
	}

	void RegisterHoldSuccess(HoldSuccessSignal _Signal)
	{
		ScoreGrade grade;
		if (_Signal.Progress >= ScorePerfectThreshold)
		{
			Combo++;
			
			grade = ScoreGrade.None;
			m_HoldPerfect++;
			AddScore(_Signal.Progress * HoldPerfectMultiplier);
		}
		else if (_Signal.Progress >= ScoreGoodThreshold)
		{
			Combo++;
			
			grade = ScoreGrade.None;
			m_HoldGood++;
			AddScore(_Signal.Progress * HoldGoodMultiplier);
		}
		else
		{
			Combo = 0;
			
			grade = ScoreGrade.Bad;
			m_HoldBad++;
			AddScore(_Signal.Progress * HoldBadMultiplier);
		}
		
		ProcessScore(grade);
	}

	void RegisterHoldFail(HoldFailSignal _Signal)
	{
		m_HoldFail++;
		
		Combo = 0;
		
		ProcessScore(ScoreGrade.Fail);
	}

	void RegisterHoldHit(HoldHitSignal _Signal)
	{
		ScoreGrade grade;
		if (_Signal.Progress >= ScorePerfectThreshold)
		{
			Combo++;
			
			grade = ScoreGrade.Perfect;
		}
		else if (_Signal.Progress >= ScoreGoodThreshold)
		{
			Combo++;
			
			grade = ScoreGrade.Good;
		}
		else
		{
			Combo = 0;
			
			grade = ScoreGrade.Bad;
		}
		
		AddScore(_Signal.Progress * HoldHitMultiplier);
		
		ProcessScore(grade);
	}

	void RegisterHoldMiss(HoldMissSignal _Signal)
	{
		Combo = 0;
		
		ProcessScore(ScoreGrade.Bad);
	}

	void AddScore(double _Score)
	{
		Score += (long)(_Score * Multiplier);
	}

	void ProcessScore(ScoreGrade _Grade = ScoreGrade.None)
	{
		ScoreSignal signal = new ScoreSignal(
			Score,
			Combo,
			Multiplier,
			Progress,
			_Grade
		);
		
		m_SignalBus.Fire(signal);
	}
}