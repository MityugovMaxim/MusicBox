using System;
using System.Collections.Generic;
using System.Linq;
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

public enum ScoreType
{
	Tap    = 0,
	Double = 1,
	Hold   = 2,
}

public enum ScoreGrade
{
	None    = 0,
	Perfect = 1,
	Great   = 2,
	Good    = 3,
	Bad     = 4,
	Miss    = 5,
	Fail    = 6,
}

[Preserve]
public class ScoreManager : IInitializable, IDisposable
{
	public class Threshold
	{
		public ScoreGrade Grade      { get; }
		public float      Progress   { get; }
		public float      Multiplier { get; }

		public Threshold(ScoreGrade _Grade, float _Progress, float _Multiplier)
		{
			Grade      = _Grade;
			Progress   = _Progress;
			Multiplier = _Multiplier;
		}
	}

	long SourceScore { get; set; }
	long TargetScore { get; set; }
	int  SourceCombo { get; set; }
	int  TargetCombo { get; set; }

	float Progress
	{
		get
		{
			int minProgress = GetMinProgress(SourceCombo);
			int maxProgress = GetMaxProgress(SourceCombo);
			return Mathf.InverseLerp(minProgress, maxProgress - 1, SourceCombo);
		}
	}

	int ComboX8 { get; set; }
	int ComboX6 { get; set; }
	int ComboX4 { get; set; }
	int ComboX2 { get; set; }

	[Inject] SignalBus        m_SignalBus;
	[Inject] ScoresProcessor  m_ScoresProcessor;
	[Inject] SongsProcessor   m_SongsProcessor;
	[Inject] ProfileProcessor m_ProfileProcessor;
	[Inject] ConfigProcessor  m_ConfigProcessor;

	string m_SongID;

	readonly List<Threshold> m_TapThresholds    = new List<Threshold>();
	readonly List<Threshold> m_DoubleThresholds = new List<Threshold>();
	readonly List<Threshold> m_HoldThresholds   = new List<Threshold>();

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<InputMissSignal>(RegisterInputMiss);
		
		m_SignalBus.Subscribe<TapSuccessSignal>(RegisterTapSuccess);
		m_SignalBus.Subscribe<TapFailSignal>(RegisterTapFail);
		
		m_SignalBus.Subscribe<DoubleSuccessSignal>(RegisterDoubleSuccess);
		m_SignalBus.Subscribe<DoubleFailSignal>(RegisterDoubleFail);
		
		m_SignalBus.Subscribe<HoldSuccessSignal>(RegisterHoldSuccess);
		m_SignalBus.Subscribe<HoldFailSignal>(RegisterHoldFail);
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
		m_SignalBus.Unsubscribe<HoldMissSignal>(RegisterHoldMiss);
	}

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		ComboX8 = m_ConfigProcessor.ComboX8;
		ComboX6 = m_ConfigProcessor.ComboX6;
		ComboX4 = m_ConfigProcessor.ComboX4;
		ComboX2 = m_ConfigProcessor.ComboX2;
		
		float perfectThreshold = m_ConfigProcessor.ScorePerfectThreshold;
		float greatThreshold   = m_ConfigProcessor.ScoreGreatThreshold;
		float goodThreshold    = m_ConfigProcessor.ScoreGoodThreshold;
		
		m_TapThresholds.Clear();
		
		m_TapThresholds.Add(new Threshold(ScoreGrade.Perfect, perfectThreshold, m_ConfigProcessor.TapPerfectMultiplier));
		m_TapThresholds.Add(new Threshold(ScoreGrade.Great, greatThreshold, m_ConfigProcessor.TapGreatMultiplier));
		m_TapThresholds.Add(new Threshold(ScoreGrade.Good, goodThreshold, m_ConfigProcessor.TapGoodMultiplier));
		m_TapThresholds.Add(new Threshold(ScoreGrade.Bad, 0, m_ConfigProcessor.TapBadMultiplier));
		
		m_DoubleThresholds.Clear();
		
		m_DoubleThresholds.Add(new Threshold(ScoreGrade.Perfect, perfectThreshold, m_ConfigProcessor.DoublePerfectMultiplier));
		m_DoubleThresholds.Add(new Threshold(ScoreGrade.Great, greatThreshold, m_ConfigProcessor.DoubleGreatMultiplier));
		m_DoubleThresholds.Add(new Threshold(ScoreGrade.Good, goodThreshold, m_ConfigProcessor.DoubleGoodMultiplier));
		m_DoubleThresholds.Add(new Threshold(ScoreGrade.Bad, 0, m_ConfigProcessor.DoubleBadMultiplier));
		
		m_HoldThresholds.Clear();
		
		m_HoldThresholds.Add(new Threshold(ScoreGrade.Perfect, perfectThreshold, m_ConfigProcessor.HoldPerfectMultiplier));
		m_HoldThresholds.Add(new Threshold(ScoreGrade.Great, greatThreshold, m_ConfigProcessor.HoldGreatMultiplier));
		m_HoldThresholds.Add(new Threshold(ScoreGrade.Good, goodThreshold, m_ConfigProcessor.HoldGoodMultiplier));
		m_HoldThresholds.Add(new Threshold(ScoreGrade.Bad, 0, m_ConfigProcessor.HoldBadMultiplier));
	}

	public void Restore()
	{
		SourceScore = 0;
		TargetScore = 0;
		
		SourceCombo = 0;
		TargetCombo = 0;
		
		ProcessScore();
	}

	public int GetAccuracy()
	{
		float accuracy = Mathf.InverseLerp(0, TargetScore, SourceScore);
		
		return Mathf.RoundToInt(accuracy * 100);
	}

	public ScoreRank GetRank()
	{
		int accuracy = GetAccuracy();
		
		return m_SongsProcessor.GetRank(m_SongID, accuracy);
	}

	public long GetScore()
	{
		return SourceScore;
	}

	public ScoreGrade GetGrade(ScoreType _Type, float _Progress)
	{
		Threshold threshold = GetThreshold(_Type, _Progress);
		
		return threshold?.Grade ?? ScoreGrade.None;
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

	Threshold GetThreshold(ScoreType _Type, float _Progress)
	{
		switch (_Type)
		{
			case ScoreType.Tap:
				return GetTapThreshold(_Progress);
			case ScoreType.Double:
				return GetDoubleThreshold(_Progress);
			case ScoreType.Hold:
				return GetHoldThreshold(_Progress);
			default:
				return null;
		}
	}

	Threshold GetTapThreshold(float _Progress)
	{
		return m_TapThresholds.FirstOrDefault(_Threshold => _Threshold.Progress <= _Progress);
	}

	Threshold GetDoubleThreshold(float _Progress)
	{
		return m_DoubleThresholds.FirstOrDefault(_Threshold => _Threshold.Progress <= _Progress);
	}

	Threshold GetHoldThreshold(float _Progress)
	{
		return m_HoldThresholds.FirstOrDefault(_Threshold => _Threshold.Progress <= _Progress);
	}

	void RegisterInputMiss()
	{
		SourceCombo = 0;
		
		ProcessScore(ScoreGrade.Miss);
	}

	void RegisterSuccess(ScoreType _Type, float _Progress)
	{
		Threshold threshold = GetThreshold(_Type, _Progress);
		
		ScoreGrade grade      = threshold?.Grade ?? ScoreGrade.None;
		float      multiplier = threshold?.Multiplier ?? 0;
		
		if (grade < ScoreGrade.Bad)
			SourceCombo++;
		else
			SourceCombo = 0;
		
		TargetCombo++;
		
		AddSourceScore(_Progress * multiplier);
		
		AddTargetScore(_Type);
		
		ProcessScore(grade);
	}

	void RegisterFail(ScoreType _Type)
	{
		SourceCombo = 0;
		
		TargetCombo++;
		
		AddTargetScore(_Type);
		
		ProcessScore(ScoreGrade.Fail);
	}

	void RegisterTapSuccess(TapSuccessSignal _Signal)
	{
		RegisterSuccess(ScoreType.Tap, _Signal.Progress);
	}

	void RegisterTapFail(TapFailSignal _Signal)
	{
		RegisterFail(ScoreType.Tap);
	}

	void RegisterDoubleSuccess(DoubleSuccessSignal _Signal)
	{
		RegisterSuccess(ScoreType.Double, _Signal.Progress);
	}

	void RegisterDoubleFail(DoubleFailSignal _Signal)
	{
		RegisterFail(ScoreType.Double);
	}

	void RegisterHoldSuccess(HoldSuccessSignal _Signal)
	{
		RegisterSuccess(ScoreType.Hold, _Signal.Progress);
	}

	void RegisterHoldFail(HoldFailSignal _Signal)
	{
		RegisterFail(ScoreType.Hold);
	}

	void RegisterHoldMiss(HoldMissSignal _Signal)
	{
		SourceCombo = 0;
		
		ProcessScore(ScoreGrade.Miss);
	}

	int GetMultiplier(int _Combo)
	{
		if (_Combo >= ComboX8)
			return 8;
		else if (_Combo >= ComboX6)
			return 6;
		else if (_Combo >= ComboX4)
			return 4;
		else if (_Combo >= ComboX2)
			return 2;
		else
			return 1;
	}

	int GetMinProgress(int _Combo)
	{
		if (_Combo >= ComboX8)
			return ComboX8;
		else if (_Combo >= ComboX6)
			return ComboX6;
		else if (_Combo >= ComboX4)
			return ComboX4;
		else if (_Combo >= ComboX2)
			return ComboX2;
		else
			return 0;
	}

	int GetMaxProgress(int _Combo)
	{
		if (_Combo >= ComboX8)
			return ComboX8;
		else if (_Combo >= ComboX6)
			return ComboX8;
		else if (_Combo >= ComboX4)
			return ComboX6;
		else if (_Combo >= ComboX2)
			return ComboX4;
		else
			return ComboX2;
	}

	void AddSourceScore(double _Score)
	{
		SourceScore += (long)(_Score * GetMultiplier(SourceCombo));
	}

	void AddTargetScore(ScoreType _Type)
	{
		Threshold threshold = GetThreshold(_Type, 1);
		
		float score = threshold?.Multiplier ?? 0;
		
		TargetScore += (long)(score * GetMultiplier(TargetCombo));
	}

	void ProcessScore(ScoreGrade _Grade = ScoreGrade.None)
	{
		ScoreSignal signal = new ScoreSignal(
			SourceScore,
			SourceCombo,
			GetMultiplier(SourceCombo),
			Progress,
			_Grade
		);
		
		m_SignalBus.Fire(signal);
	}
}