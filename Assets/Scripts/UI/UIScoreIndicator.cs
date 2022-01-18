using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class UIScoreIndicator : UIEntity, IInitializable, IDisposable
{
	[SerializeField] UIUnitLabel m_ScoreLabel;
	[SerializeField] float       m_Duration = 0.15f;

	SignalBus m_SignalBus;

	long        m_Score;
	IEnumerator m_ScoreRoutine;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	public void Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Subscribe<LevelScoreSignal>(RegisterLevelScore);
	}

	public void Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Unsubscribe<LevelScoreSignal>(RegisterLevelScore);
	}

	void RegisterLevelStart()
	{
		Restore();
	}

	void RegisterLevelRestart()
	{
		Restore();
	}

	void RegisterLevelScore(LevelScoreSignal _Signal)
	{
		SetScore(_Signal.Score);
	}

	void SetScore(long _Score, bool _Instant = false)
	{
		if (m_ScoreRoutine != null)
			StopCoroutine(m_ScoreRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_ScoreRoutine = ScoreRoutine(_Score);
			
			StartCoroutine(m_ScoreRoutine);
		}
		else
		{
			m_Score = _Score;
			
			m_ScoreLabel.Value = m_Score;
		}
	}

	void Restore()
	{
		SetScore(0, true);
	}

	IEnumerator ScoreRoutine(long _Score)
	{
		long source = m_Score;
		long target = _Score;
		
		if (source != target)
		{
			long  delta = target - source;
			float time  = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				double phase = time / m_Duration;
				
				m_Score = source + (long)(delta * phase);
				
				m_ScoreLabel.Value = m_Score;
			}
		}
		
		m_Score = target;
		
		m_ScoreLabel.Value = m_Score;
	}
}