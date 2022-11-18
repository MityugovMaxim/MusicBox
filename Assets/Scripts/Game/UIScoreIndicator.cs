using System.Collections;
using UnityEngine;
using Zenject;

public class UIScoreIndicator : UIOrder
{
	public override int Thickness => 1;

	[SerializeField] UIUnitLabel m_ScoreLabel;
	[SerializeField] float       m_Duration = 0.15f;

	[Inject] ScoreController m_ScoreController;

	long m_Score;

	IEnumerator m_ScoreRoutine;

	protected override void Awake()
	{
		base.Awake();
		
		m_ScoreController.OnScoreChange += OnScoresChange;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_ScoreController.OnScoreChange -= OnScoresChange;
	}

	void OnScoresChange(long _Score)
	{
		m_Score = _Score;
		
		if (m_ScoreRoutine != null)
			StopCoroutine(m_ScoreRoutine);
		
		if (m_Score > m_ScoreLabel.Value && gameObject.activeInHierarchy)
		{
			m_ScoreRoutine = UnityRoutine.UnitRoutine(m_ScoreLabel, m_Score, m_Duration, EaseFunction.EaseOut);
			
			StartCoroutine(m_ScoreRoutine);
		}
		else
		{
			m_ScoreLabel.Value = m_Score;
		}
	}
}
