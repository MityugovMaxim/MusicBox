using UnityEngine;
using Zenject;

public class UIAccuracyIndicator : UIOrder
{
	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField] Animator m_AccuracyPerfect;
	[SerializeField] Animator m_AccuracyBad;

	[Inject] ScoreManager m_ScoreManager;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		if (m_ScoreManager != null)
			m_ScoreManager.OnComboChanged += OnComboChanged;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		if (m_ScoreManager != null)
			m_ScoreManager.OnComboChanged -= OnComboChanged;
	}

	void OnComboChanged(int _Combo, ScoreGrade _Grade)
	{
		switch (_Grade)
		{
			case ScoreGrade.Perfect:
				m_AccuracyPerfect.SetTrigger(m_PlayParameterID);
				m_AccuracyBad.SetTrigger(m_RestoreParameterID);
				break;
			
			case ScoreGrade.Bad:
				m_AccuracyBad.SetTrigger(m_PlayParameterID);
				m_AccuracyPerfect.SetTrigger(m_RestoreParameterID);
				break;
		}
	}
}