using UnityEngine;
using Zenject;

public class UIAccuracyIndicator : UIEntity
{
	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField] Animator m_AccuracyPerfect;
	[SerializeField] Animator m_AccuracyBad;

	[Inject] SignalBus m_SignalBus;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_SignalBus.Subscribe<ScoreSignal>(RegisterScore);
	}

	void RegisterScore(ScoreSignal _Signal)
	{
		switch (_Signal.Grade)
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