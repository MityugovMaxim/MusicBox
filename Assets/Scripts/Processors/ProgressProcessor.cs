using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ProgressProcessor : IInitializable, IDisposable
{
	const string EXP_PROGRESS_KEY = "EXP_PROGRESS";

	public long ExpProgress { get; private set; }

	readonly SignalBus                     m_SignalBus;
	readonly ScoreProcessor                m_ScoreProcessor;
	readonly Dictionary<string, LevelInfo> m_LevelInfos = new Dictionary<string,LevelInfo>();

	[Inject]
	public ProgressProcessor(
		SignalBus      _SignalBus,
		ScoreProcessor _ScoreProcessor
	)
	{
		m_SignalBus      = _SignalBus;
		m_ScoreProcessor = _ScoreProcessor;
		
		LevelRegistry levelRegistry = Registry.Load<LevelRegistry>("level_registry");
		if (levelRegistry != null)
		{
			foreach (LevelInfo levelInfo in levelRegistry)
			{
				if (levelInfo == null || !levelInfo.Active)
					continue;
				
				m_LevelInfos[levelInfo.ID] = levelInfo;
			}
		}
		
		LoadExpProgress();
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelFinishSignal>(RegisterLevelFinish);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelFinishSignal>(RegisterLevelFinish);
	}

	void RegisterLevelFinish(LevelFinishSignal _Signal)
	{
		ScoreData scoreData   = m_ScoreProcessor.ScoreData;
		long      expPayout   = GetExpPayout(_Signal.LevelID) * GetExpMultiplier(scoreData.Rank);
		long      expProgress = ExpProgress + expPayout;
		
		foreach (LevelInfo levelInfo in m_LevelInfos.Values)
		{
			if (levelInfo.Locked && levelInfo.ExpRequired > ExpProgress && levelInfo.ExpRequired <= expProgress)
				m_SignalBus.Fire(new LevelUnlockSignal(levelInfo.ID));
		}
		
		ExpProgress = expProgress;
		
		SaveExpProgress();
	}

	public long GetExpPayout(string _LevelID)
	{
		LevelInfo levelInfo = GetLevelInfo(_LevelID);
		
		if (levelInfo == null)
		{
			Debug.LogErrorFormat("[ProgressProcessor] Get exp payout failed. Level info for ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return levelInfo.ExpPayout;
	}

	public int GetExpMultiplier(ScoreRank _Rank)
	{
		switch (_Rank)
		{
			case ScoreRank.S:
				return 5;
			case ScoreRank.A:
				return 3;
			case ScoreRank.B:
				return 2;
			case ScoreRank.C:
				return 1;
			default:
				return 0;
		}
	}

	public long GetExpRequired(string _LevelID)
	{
		LevelInfo levelInfo = GetLevelInfo(_LevelID);
		
		if (levelInfo == null)
		{
			Debug.LogErrorFormat("[ProgressProcessor] Get exp required failed. Level info for ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return levelInfo.Locked ? levelInfo.ExpRequired : 0;
	}

	public bool IsLevelLocked(string _LevelID)
	{
		LevelInfo levelInfo = GetLevelInfo(_LevelID);
		
		if (levelInfo == null)
		{
			Debug.LogErrorFormat("[ProgressProcessor] Level lock check failed. Level info for ID '{0}' is null.", _LevelID);
			return false;
		}
		
		return levelInfo.Locked && levelInfo.ExpRequired > ExpProgress;
	}

	public bool IsLevelUnlocked(string _LevelID)
	{
		LevelInfo levelInfo = GetLevelInfo(_LevelID);
		
		if (levelInfo == null)
		{
			Debug.LogErrorFormat("[ProgressProcessor] Level unlock check failed. LevelInfo for ID '{0}' is null.", _LevelID);
			return true;
		}
		
		return !levelInfo.Locked || levelInfo.ExpRequired <= ExpProgress;
	}

	LevelInfo GetLevelInfo(string _LevelID)
	{
		if (string.IsNullOrEmpty(_LevelID))
		{
			Debug.LogError("[ProgressProcessor] Get level info failed. Level ID is null or empty.");
			return null;
		}
		
		if (!m_LevelInfos.ContainsKey(_LevelID))
		{
			Debug.LogErrorFormat("[ProgressProcessor] Get level info failed. Level with ID '{0}' not found.", _LevelID);
			return null;
		}
		
		return m_LevelInfos[_LevelID];
	}

	void LoadExpProgress()
	{
		string data = PlayerPrefs.GetString(EXP_PROGRESS_KEY, string.Empty);
		
		ExpProgress = long.TryParse(data, out long progress) ? progress : 0;
	}

	void SaveExpProgress()
	{
		string data = ExpProgress.ToString();
		
		PlayerPrefs.SetString(EXP_PROGRESS_KEY, data);
	}
}
