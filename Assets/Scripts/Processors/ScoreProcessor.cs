using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ScoreProcessor : IInitializable, IDisposable
{
	const int X4_COMBO = 60;
	const int X3_COMBO = 30;
	const int X2_COMBO = 15;

	public ScoreData ScoreData { get; private set; }

	int Multiplier
	{
		get
		{
			if (Combo >= X4_COMBO)
				return 4;
			else if (Combo >= X3_COMBO)
				return 3;
			else if (Combo >= X2_COMBO)
				return 2;
			else
				return 1;
		}
	}

	int Combo
	{
		get => m_Combo;
		set
		{
			if (m_Combo == value)
				return;
			
			int sourceMultiplier = Multiplier;
			
			m_Combo = value;
			
			int targetMultiplier = Multiplier;
			
			if (sourceMultiplier != targetMultiplier)
				m_SignalBus.Fire(new LevelComboSignal(targetMultiplier));
		}
	}

	readonly SignalBus m_SignalBus;

	readonly Dictionary<string, ScoreData> m_LastScore = new Dictionary<string, ScoreData>();
	readonly Dictionary<string, ScoreData> m_BestScore = new Dictionary<string, ScoreData>();

	int m_Combo;

	[Inject]
	public ScoreProcessor(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
		
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
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
		
		m_SignalBus.Unsubscribe<HoldSuccessSignal>(RegisterHoldSuccess);
		m_SignalBus.Unsubscribe<HoldFailSignal>(RegisterHoldFail);
		m_SignalBus.Unsubscribe<HoldHitSignal>(RegisterHoldHit);
		m_SignalBus.Unsubscribe<HoldMissSignal>(RegisterHoldMiss);
		
		m_SignalBus.Unsubscribe<TapSuccessSignal>(RegisterTapSuccess);
		m_SignalBus.Unsubscribe<TapFailSignal>(RegisterTapFail);
		
		m_SignalBus.Unsubscribe<DoubleSuccessSignal>(RegisterDoubleSuccess);
		m_SignalBus.Unsubscribe<DoubleFailSignal>(RegisterDoubleFail);
	}

	public int GetLastAccuracy(string _LevelID)
	{
		ScoreData scoreData = LoadLastScore(_LevelID);
		
		return scoreData.Accuracy;
	}

	public long GetLastScore(string _LevelID)
	{
		ScoreData scoreData = LoadLastScore(_LevelID);
		
		return scoreData.Score;
	}

	public ScoreRank GetLastRank(string _LevelID)
	{
		ScoreData scoreData = LoadLastScore(_LevelID);
		
		return scoreData.Rank;
	}

	public int GetBestAccuracy(string _LevelID)
	{
		ScoreData scoreData = LoadBestScore(_LevelID);
		
		return scoreData.Accuracy;
	}

	public long GetBestScore(string _LevelID)
	{
		ScoreData scoreData = LoadBestScore(_LevelID);
		
		return scoreData.Score;
	}

	public ScoreRank GetBestRank(string _LevelID)
	{
		ScoreData scoreData = LoadBestScore(_LevelID);
		
		return scoreData.Rank;
	}

	public ScoreData LoadLastScore(string _LevelID)
	{
		if (string.IsNullOrEmpty(_LevelID))
		{
			Debug.LogError("[ScoreProcessor] Load last score failed. Level ID is null or empty.");
			return new ScoreData();
		}
		
		if (m_LastScore.ContainsKey(_LevelID))
			return m_LastScore[_LevelID];
		
		string key = $"[ScoreData] Last {_LevelID}";
		
		ScoreData scoreData = LoadScore(key);
		
		if (scoreData == null)
		{
			Debug.LogErrorFormat("[ScoreProcessor] Load last score failed. Saved score data for level with ID '{0}' deserialization failed.", _LevelID);
			return new ScoreData();
		}
		
		m_LastScore[_LevelID] = scoreData;
		
		return scoreData;
	}

	public void SaveLastScore(string _LevelID, ScoreData _ScoreData)
	{
		if (string.IsNullOrEmpty(_LevelID))
		{
			Debug.LogError("[ScoreProcessor] Save score failed. Score data level ID is null or empty.");
			return;
		}
		
		if (_ScoreData == null)
		{
			Debug.LogErrorFormat("[ScoreProcessor] Save score failed. Score data is null for level with ID '{0}'.", _LevelID);
			return;
		}
		
		m_LastScore[_LevelID] = _ScoreData;
		
		string key = $"[ScoreData] Last {_LevelID}";
		
		SaveScore(key, _ScoreData);
	}

	public ScoreData LoadBestScore(string _LevelID)
	{
		if (string.IsNullOrEmpty(_LevelID))
		{
			Debug.LogError("[ScoreProcessor] Load best score failed. Level ID is null or empty.");
			return new ScoreData();
		}
		
		if (m_BestScore.ContainsKey(_LevelID))
			return m_BestScore[_LevelID];
		
		string key = $"[ScoreData] Best {_LevelID}";
		
		ScoreData scoreData = LoadScore(key);
		
		if (scoreData == null)
		{
			Debug.LogErrorFormat("[ScoreProcessor] Load best score failed. Saved score data for level with ID '{0}' deserialization failed.", _LevelID);
			return new ScoreData();
		}
		
		m_BestScore[_LevelID] = scoreData;
		
		return scoreData;
	}

	public void SaveBestScore(string _LevelID, ScoreData _ScoreData)
	{
		if (string.IsNullOrEmpty(_LevelID))
		{
			Debug.LogError("[ScoreProcessor] Save best score failed. Score data level ID is null or empty.");
			return;
		}
		
		if (_ScoreData == null)
		{
			Debug.LogErrorFormat("[ScoreProcessor] Save best score failed. Score data is null for level with ID '{0}'.", _LevelID);
			return;
		}
		
		if (m_BestScore.TryGetValue(_LevelID, out ScoreData scoreData) && scoreData != null && scoreData.Rank > _ScoreData.Rank)
			return;
		
		m_BestScore[_LevelID] = _ScoreData;
		
		string key = $"[ScoreData] Best {_LevelID}";
		
		SaveScore(key, _ScoreData);
	}

	static ScoreData LoadScore(string _Key)
	{
		if (string.IsNullOrEmpty(_Key))
		{
			Debug.LogError("[ScoreProcessor] Load score failed. Key is null or empty.");
			return new ScoreData();
		}
		
		string data = PlayerPrefs.GetString(_Key);
		
		if (string.IsNullOrEmpty(data))
			return new ScoreData();
		
		ScoreData scoreData = JsonUtility.FromJson<ScoreData>(data);
		
		if (scoreData == null)
		{
			Debug.LogErrorFormat("[ScoreProcessor] Load score failed. Saved score data for key '{0}' deserialization failed.", _Key);
			return new ScoreData();
		}
		
		return scoreData;
	}

	static void SaveScore(string _Key, ScoreData _ScoreData)
	{
		if (string.IsNullOrEmpty(_Key))
		{
			Debug.LogError("[ScoreProcessor] Save score failed. Key is null or empty.");
			return;
		}
		
		if (_ScoreData == null)
		{
			Debug.LogErrorFormat("[ScoreProcessor] Save score failed. Score data is null for key '{0}'.", _Key);
			return;
		}
		
		string data = JsonUtility.ToJson(_ScoreData);
		
		PlayerPrefs.SetString(_Key, data);
	}

	void RegisterLevelStart()
	{
		m_Combo = 0;
		
		ScoreData = new ScoreData();
	}

	void RegisterLevelRestart()
	{
		m_Combo = 0;
		
		ScoreData = new ScoreData();
	}

	void RegisterHoldSuccess(HoldSuccessSignal _Signal)
	{
		Combo++;
		
		ScoreData.RegisterHoldSuccess(_Signal.Progress, Multiplier);
		
		m_SignalBus.Fire(new LevelScoreSignal(ScoreData.Score));
	}

	void RegisterHoldFail(HoldFailSignal _Signal)
	{
		Combo = 0;
		
		ScoreData.RegisterHoldFail(_Signal.Progress, Multiplier);
		
		m_SignalBus.Fire(new LevelScoreSignal(ScoreData.Score));
	}

	void RegisterHoldHit(HoldHitSignal _Signal)
	{
		ScoreData.RegisterHoldHit();
		
		m_SignalBus.Fire(new LevelScoreSignal(ScoreData.Score));
	}

	void RegisterHoldMiss(HoldMissSignal _Signal)
	{
		Combo = 0;
		
		ScoreData.RegisterHoldMiss();
		
		m_SignalBus.Fire(new LevelScoreSignal(ScoreData.Score));
	}

	void RegisterTapSuccess(TapSuccessSignal _Signal)
	{
		Combo++;
		
		ScoreData.RegisterTapSuccess(_Signal.Progress, Multiplier);
		
		m_SignalBus.Fire(new LevelScoreSignal(ScoreData.Score));
	}

	void RegisterTapFail(TapFailSignal _Signal)
	{
		Combo = 0;
		
		ScoreData.RegisterTapFail();
		
		m_SignalBus.Fire(new LevelScoreSignal(ScoreData.Score));
	}

	void RegisterDoubleSuccess(DoubleSuccessSignal _Signal)
	{
		Combo++;
		
		ScoreData.RegisterDoubleSuccess(_Signal.Progress, Multiplier);
		
		m_SignalBus.Fire(new LevelScoreSignal(ScoreData.Score));
	}

	void RegisterDoubleFail(DoubleFailSignal _Signal)
	{
		Combo = 0;
		
		ScoreData.RegisterDoubleFail();
		
		m_SignalBus.Fire(new LevelScoreSignal(ScoreData.Score));
	}
}