using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

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
public class ScoreManager
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
			return minProgress < maxProgress ? Mathf.InverseLerp(minProgress, maxProgress - 1, SourceCombo) : 1;
		}
	}

	int ComboX8 { get; set; }
	int ComboX6 { get; set; }
	int ComboX4 { get; set; }
	int ComboX2 { get; set; }

	public event Action<long>                  OnScoreChanged;
	public event Action<int, ScoreGrade>       OnComboChanged;
	public event Action<int, float>            OnMultiplierChanged;
	public event Action<ScoreType, ScoreGrade> OnHit;

	[Inject] ScoresProcessor  m_ScoresProcessor;
	[Inject] SongsProcessor   m_SongsProcessor;
	[Inject] ProfileProcessor m_ProfileProcessor;
	[Inject] ConfigProcessor  m_ConfigProcessor;

	string m_SongID;

	readonly List<Threshold>             m_TapThresholds    = new List<Threshold>();
	readonly List<Threshold>             m_DoubleThresholds = new List<Threshold>();
	readonly List<Threshold>             m_HoldThresholds   = new List<Threshold>();
	readonly Dictionary<ScoreGrade, int> m_Statistics       = new Dictionary<ScoreGrade, int>();

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
		
		m_Statistics.Clear();
		
		ProcessScore();
	}

	public int GetAccuracy()
	{
		if (SourceScore == TargetScore)
			return 100;
		
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

	public long GetScore(int _Accuracy)
	{
		return TargetScore / 100 * _Accuracy;
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

	public int GetStatistics(ScoreGrade _Grade)
	{
		return m_Statistics.TryGetValue(_Grade, out int count) ? count : 0;
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
		return GetThreshold(m_TapThresholds, _Progress);
	}

	Threshold GetDoubleThreshold(float _Progress)
	{
		return GetThreshold(m_DoubleThresholds, _Progress);
	}

	Threshold GetHoldThreshold(float _Progress)
	{
		return GetThreshold(m_HoldThresholds, _Progress);
	}

	static Threshold GetThreshold(List<Threshold> _Thresholds, float _Progress)
	{
		if (_Thresholds == null || _Thresholds.Count == 0)
			return null;
		
		foreach (Threshold threshold in _Thresholds)
		{
			if (threshold.Progress <= _Progress)
				return threshold;
		}
		
		return _Thresholds[^1];
	}

	void RegisterHit(ScoreType _Type, float _Progress)
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
		
		OnHit?.Invoke(_Type, grade);
	}

	void RegisterFail(ScoreType _Type)
	{
		SourceCombo = 0;
		
		TargetCombo++;
		
		AddTargetScore(_Type);
		
		ProcessScore(ScoreGrade.Fail);
	}

	public void TapHit(float _Progress)
	{
		RegisterHit(ScoreType.Tap, _Progress);
	}

	public void DoubleHit(float _Progress)
	{
		RegisterHit(ScoreType.Double, _Progress);
	}

	public void HoldHit(float _Progress)
	{
		Threshold threshold = GetThreshold(ScoreType.Hold, _Progress);
		
		ScoreGrade grade = threshold?.Grade ?? ScoreGrade.None;
		
		OnHit?.Invoke(ScoreType.Hold, grade);
	}

	public void HoldRelease(float _Progress, float _Length)
	{
		Threshold threshold = GetThreshold(ScoreType.Hold, _Progress);
		
		float      progress   = threshold?.Progress ?? 0;
		ScoreGrade grade      = threshold?.Grade ?? ScoreGrade.None;
		float      multiplier = threshold?.Multiplier ?? 0;
		
		if (grade < ScoreGrade.Bad)
			SourceCombo++;
		else
			SourceCombo = 0;
		
		TargetCombo++;
		
		AddSourceScore(_Length * multiplier);
		
		AddTargetScore(ScoreType.Hold);
		
		float length = 1.0f - _Progress + _Length;
		if (length < progress)
		{
			switch (grade)
			{
				case ScoreGrade.Perfect:
					grade = ScoreGrade.Good;
					break;
				case ScoreGrade.Great:
					grade = ScoreGrade.Good;
					break;
			}
		}
		
		ProcessScore(grade);
	}

	public void TapFail() => RegisterFail(ScoreType.Tap);

	public void DoubleFail() => RegisterFail(ScoreType.Double);

	public void HoldFail() => RegisterFail(ScoreType.Double);

	public void Miss()
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
		
		float score    = threshold?.Multiplier ?? 0;
		float progress = threshold?.Progress ?? 1;
		
		TargetScore += (long)(score * GetMultiplier(TargetCombo) * progress);
	}

	void ProcessScore(ScoreGrade _Grade = ScoreGrade.None)
	{
		if (m_Statistics.ContainsKey(_Grade))
			m_Statistics[_Grade]++;
		else
			m_Statistics[_Grade] = 1;
		
		OnScoreChanged?.Invoke(SourceScore);
		OnComboChanged?.Invoke(SourceCombo, _Grade);
		OnMultiplierChanged?.Invoke(GetMultiplier(SourceCombo), Progress);
	}
}