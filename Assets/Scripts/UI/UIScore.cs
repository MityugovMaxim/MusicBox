using System;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIScore : UIEntity
{
	static readonly int m_SuccessParameterID = Animator.StringToHash("Success");
	static readonly int m_FailParameterID    = Animator.StringToHash("Fail");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField, Range(0, 1)] float       m_AccuracyPhase;
	[SerializeField, Range(0, 1)] float       m_ScorePhase;
	[SerializeField, Range(0, 1)] float       m_ExpPayoutPhase;
	[SerializeField]              TMP_Text    m_AccuracyLabel;
	[SerializeField]              TMP_Text    m_ScoreLabel;
	[SerializeField]              UIExpLabel  m_ExpPayoutLabel;
	[SerializeField]              UIScoreRank m_ScoreRank;

	ScoreProcessor    m_ScoreProcessor;
	ProgressProcessor m_ProgressProcessor;
	HapticProcessor   m_HapticProcessor;

	string           m_LevelID;
	int              m_Accuracy;
	long             m_Score;
	long             m_ExpPayout;
	int              m_ExpMultiplier;
	Animator         m_Animator;
	StateBehaviour[] m_PlayStates;
	Action           m_Finished;

	[Inject]
	public void Construct(
		ScoreProcessor    _ScoreProcessor,
		ProgressProcessor _ProgressProcessor,
		HapticProcessor   _HapticProcessor
	)
	{
		m_ScoreProcessor    = _ScoreProcessor;
		m_ProgressProcessor = _ProgressProcessor;
		m_HapticProcessor   = _HapticProcessor;
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
		
		m_PlayStates = StateBehaviour.GetBehaviours(m_Animator, "play");
		if (m_PlayStates != null)
		{
			foreach (StateBehaviour state in m_PlayStates)
				state.OnComplete += InvokeFinished;
		}
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessScorePhase();
		
		ProcessAccuracyPhase();
		
		ProcessPayoutPhase();
	}

	public void Setup(string _LevelID)
	{
		m_LevelID       = _LevelID;
		m_Accuracy      = m_ScoreProcessor.GetLastAccuracy(m_LevelID);
		m_Score         = m_ScoreProcessor.GetLastScore(m_LevelID);
		m_ExpPayout     = m_ProgressProcessor.GetExpPayout(m_LevelID);
		m_ExpMultiplier = m_ProgressProcessor.GetExpMultiplier(m_ScoreProcessor.GetLastRank(m_LevelID));
		
		if (m_ScoreRank != null)
			m_ScoreRank.Setup(_LevelID);
	}

	public void Restore()
	{
		if (m_Animator == null)
			return;
		
		m_Animator.ResetTrigger(m_SuccessParameterID);
		m_Animator.ResetTrigger(m_FailParameterID);
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	public void Play(Action _Finished = null)
	{
		if (m_Animator == null)
			return;
		
		InvokeFinished();
		
		m_Finished = _Finished;
		
		ScoreRank rank = m_ScoreProcessor.GetLastRank(m_LevelID);
		
		if (rank == ScoreRank.None)
			m_Animator.SetTrigger(m_FailParameterID);
		else
			m_Animator.SetTrigger(m_SuccessParameterID);
	}

	[Preserve]
	void Haptic(Haptic.Type _HapticType)
	{
		m_HapticProcessor.Process(_HapticType);
	}

	void ProcessScorePhase()
	{
		if (m_ScoreLabel != null)
			m_ScoreLabel.text = ((long)Math.Round(m_Score * m_ScorePhase)).ToString();
	}

	void ProcessAccuracyPhase()
	{
		if (m_AccuracyLabel != null)
			m_AccuracyLabel.text = Mathf.RoundToInt(m_Accuracy * m_AccuracyPhase).ToString();
	}

	void ProcessPayoutPhase()
	{
		if (m_ExpPayoutLabel != null)
		{
			m_ExpPayoutLabel.Exp        = (long)Math.Round(m_ExpPayout * m_ExpPayoutPhase);
			m_ExpPayoutLabel.Multiplier = m_ExpMultiplier;
		}
	}

	void InvokeFinished()
	{
		Action action = m_Finished;
		m_Finished = null;
		action?.Invoke();
	}
}