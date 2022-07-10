using System.Collections;
using UnityEngine;
using Zenject;

public class UIScoreIndicator : UIOrder
{
	public override int Thickness => 1;

	[SerializeField] UIUnitLabel m_ScoreLabel;
	[SerializeField] float       m_Duration = 0.15f;

	[Inject] ScoreManager m_ScoreManager;

	long m_Score;

	IEnumerator m_ScoreRoutine;

	protected override void Awake()
	{
		base.Awake();
		
		if (m_ScoreManager != null)
			m_ScoreManager.OnScoreChanged += OnScoreChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		if (m_ScoreManager != null)
			m_ScoreManager.OnScoreChanged -= OnScoreChanged;
	}

	void OnScoreChanged(long _Score)
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