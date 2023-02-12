using UnityEngine;
using Zenject;

public class UIAccuracyIndicator : UIOrder
{
	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField] Animator m_AccuracyPerfect;
	[SerializeField] Animator m_AccuracyBad;

	[Inject] ScoreController m_ScoreController;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_ScoreController.OnHit.AddListener(OnHit);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_ScoreController.OnHit.RemoveListener(OnHit);
	}

	void OnHit(ScoreType _ScoreType, ScoreGrade _ScoreGrade)
	{
		switch (_ScoreGrade)
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
