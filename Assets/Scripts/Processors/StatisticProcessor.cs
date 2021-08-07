using System;
using System.Collections.Generic;
using UnityEngine.Analytics;
using Zenject;

public class StatisticProcessor : IInitializable, IDisposable
{
	readonly SignalBus      m_SignalBus;
	readonly ScoreProcessor m_ScoreProcessor;

	[Inject]
	public StatisticProcessor(
		SignalBus      _SignalBus,
		ScoreProcessor _ScoreProcessor
	)
	{
		m_SignalBus      = _SignalBus;
		m_ScoreProcessor = _ScoreProcessor;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelFinishSignal>(RegisterLevelFinish);
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Subscribe<LevelExitSignal>(RegisterLevelExit);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelFinishSignal>(RegisterLevelFinish);
		m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Unsubscribe<LevelExitSignal>(RegisterLevelExit);
	}

	void RegisterLevelStart(LevelStartSignal _Signal)
	{
		Analytics.CustomEvent(
			"level_start",
			new Dictionary<string, object>()
			{
				{ "level_id", _Signal.LevelID },
			}
		);
	}

	void RegisterLevelFinish(LevelFinishSignal _Signal)
	{
		ScoreData scoreData = m_ScoreProcessor.ScoreData;
		
		Analytics.CustomEvent(
			"level_finish",
			new Dictionary<string, object>()
			{
				{ "level_id", _Signal.LevelID },
				{ "score", scoreData.Score },
				{ "accuracy", scoreData.Accuracy },
				{ "rank", scoreData.Rank.ToString() },
			}
		);
	}

	void RegisterLevelRestart(LevelRestartSignal _Signal)
	{
		Analytics.CustomEvent(
			"level_restart",
			new Dictionary<string, object>()
			{
				{ "level_id", _Signal.LevelID },
			}
		);
	}

	void RegisterLevelExit(LevelExitSignal _Signal)
	{
		Analytics.CustomEvent(
			"level_restart",
			new Dictionary<string, object>()
			{
				{ "level_id", _Signal.LevelID },
			}
		);
	}

	public void LogLevelLike(string _LevelID)
	{
		long      score    = m_ScoreProcessor.GetLastScore(_LevelID);
		int       accuracy = m_ScoreProcessor.GetLastAccuracy(_LevelID);
		ScoreRank rank     = m_ScoreProcessor.GetLastRank(_LevelID);
		
		Analytics.CustomEvent(
			"level_like",
			new Dictionary<string, object>()
			{
				{ "level_id", _LevelID },
				{ "score", score },
				{ "accuracy", accuracy },
				{ "rank", rank.ToString() },
			}
		);
	}

	public void LogLevelDislike(string _LevelID)
	{
		long      score    = m_ScoreProcessor.GetLastScore(_LevelID);
		int       accuracy = m_ScoreProcessor.GetLastAccuracy(_LevelID);
		ScoreRank rank     = m_ScoreProcessor.GetLastRank(_LevelID);
		
		Analytics.CustomEvent(
			"level_dislike",
			new Dictionary<string, object>()
			{
				{ "level_id", _LevelID },
				{ "score", score },
				{ "accuracy", accuracy },
				{ "rank", rank.ToString() },
			}
		);
	}
}