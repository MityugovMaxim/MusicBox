using TMPro;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIScore : UIEntity
{
	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField, Range(0, 1)] float       m_AccuracyPhase;
	[SerializeField, Range(0, 1)] float       m_ScorePhase;
	[SerializeField]              TMP_Text    m_AccuracyLabel;
	[SerializeField]              TMP_Text    m_ScoreLabel;
	[SerializeField]              UIScoreRank m_ScoreRank;

	ScoreProcessor m_ScoreProcessor;
	float          m_Accuracy;
	float          m_Score;
	Animator       m_Animator;

	[Inject]
	public void Construct(ScoreProcessor _ScoreProcessor)
	{
		m_ScoreProcessor = _ScoreProcessor;
	}

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessScorePhase();
		
		ProcessAccuracyPhase();
	}

	public void Setup(string _LevelID)
	{
		m_Accuracy = m_ScoreProcessor.GetLastAccuracy(_LevelID);
		m_Score    = m_ScoreProcessor.GetLastScore(_LevelID);
		
		if (m_ScoreRank != null)
			m_ScoreRank.Setup(_LevelID);
	}

	public void Restore()
	{
		if (m_Animator != null)
		{
			m_Animator.ResetTrigger(m_PlayParameterID);
			m_Animator.SetTrigger(m_RestoreParameterID);
		}
	}

	public void Play()
	{
		if (m_Animator != null)
			m_Animator.SetTrigger(m_PlayParameterID);
	}

	void ProcessScorePhase()
	{
		if (m_ScoreLabel != null)
			m_ScoreLabel.text = Mathf.RoundToInt(m_Score * m_ScorePhase).ToString();
	}

	void ProcessAccuracyPhase()
	{
		if (m_AccuracyLabel != null)
			m_AccuracyLabel.text = Mathf.RoundToInt(m_Accuracy * m_AccuracyPhase * 100).ToString();
	}
}