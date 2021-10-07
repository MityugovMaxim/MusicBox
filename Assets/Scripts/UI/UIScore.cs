using System;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIScore : UIEntity
{
	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_RankParameterID    = Animator.StringToHash("Rank");
	static readonly int m_FastParameterID    = Animator.StringToHash("Fast");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField, Range(0, 1)] float m_AccuracyPhase;
	[SerializeField, Range(0, 1)] float m_ScorePhase;
	[SerializeField, Range(0, 1)] float m_ExpPayoutPhase;

	[SerializeField] TMP_Text      m_AccuracyLabel;
	[SerializeField] TMP_Text      m_ScoreLabel;
	[SerializeField] UICoinsLabel    m_PayoutLabel;
	[SerializeField] UICoinsLabel    m_RankSPayoutLabel;
	[SerializeField] UICoinsLabel    m_RankAPayoutLabel;
	[SerializeField] UICoinsLabel    m_RankBPayoutLabel;
	[SerializeField] UICoinsLabel    m_RankCPayoutLabel;
	[SerializeField] RectTransform m_RankSIcon;
	[SerializeField] RectTransform m_RankAIcon;
	[SerializeField] RectTransform m_RankBIcon;
	[SerializeField] RectTransform m_RankCIcon;

	ScoreProcessor    m_ScoreProcessor;
	ProfileProcessor m_ProfileProcessor;
	HapticProcessor   m_HapticProcessor;

	string           m_LevelID;
	ScoreRank        m_Rank;
	int              m_Accuracy;
	long             m_Score;
	long             m_ExpPayout;
	Animator         m_Animator;
	StateBehaviour[] m_PlayStates;
	Action           m_Finished;

	[Inject]
	public void Construct(
		ScoreProcessor    _ScoreProcessor,
		ProfileProcessor _ProfileProcessor,
		HapticProcessor   _HapticProcessor
	)
	{
		m_ScoreProcessor    = _ScoreProcessor;
		m_ProfileProcessor = _ProfileProcessor;
		m_HapticProcessor   = _HapticProcessor;
	}

	protected override void Awake()
	{
		base.Awake();
		
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
		
		ProcessScore();
		
		ProcessAccuracy();
		
		ProcessExpPayout();
	}

	public void Setup(string _LevelID)
	{
		m_LevelID  = _LevelID;
		m_Accuracy = m_ScoreProcessor.Accuracy;
		m_Score    = m_ScoreProcessor.Score;
		m_Rank     = m_ScoreProcessor.Rank;
		
		m_ExpPayout = 0;
		foreach (ScoreRank rank in Enum.GetValues(typeof(ScoreRank)))
		{
			if (rank != ScoreRank.None && rank <= m_Rank)
				m_ExpPayout += m_ProfileProcessor.GetPayout(m_LevelID, rank);
		}
		
		m_RankSPayoutLabel.Coins = m_ProfileProcessor.GetPayout(m_LevelID, ScoreRank.Platinum);
		m_RankAPayoutLabel.Coins = m_ProfileProcessor.GetPayout(m_LevelID, ScoreRank.Gold);
		m_RankBPayoutLabel.Coins = m_ProfileProcessor.GetPayout(m_LevelID, ScoreRank.Silver);
		m_RankCPayoutLabel.Coins = m_ProfileProcessor.GetPayout(m_LevelID, ScoreRank.Bronze);
		
		m_RankCIcon.SetAsLastSibling();
		m_RankBIcon.SetAsLastSibling();
		m_RankAIcon.SetAsLastSibling();
		m_RankSIcon.SetAsLastSibling();
		
		switch (m_Rank)
		{
			case ScoreRank.Platinum:
				m_RankSIcon.SetAsLastSibling();
				break;
			case ScoreRank.Gold:
				m_RankAIcon.SetAsLastSibling();
				break;
			case ScoreRank.Silver:
				m_RankBIcon.SetAsLastSibling();
				break;
			case ScoreRank.Bronze:
				m_RankCIcon.SetAsLastSibling();
				break;
		}
	}

	public void Restore()
	{
		if (m_Animator == null)
			return;
		
		m_Animator.ResetTrigger(m_PlayParameterID);
		m_Animator.SetBool(m_FastParameterID, false);
		m_Animator.SetInteger(m_RankParameterID, 0);
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	public void Play(Action _Finished = null)
	{
		if (m_Animator == null)
			return;
		
		m_Finished = _Finished;
		
		m_Animator.SetBool(m_FastParameterID, m_ExpPayout == 0);
		m_Animator.SetInteger(m_RankParameterID, (int)m_Rank);
		m_Animator.SetTrigger(m_PlayParameterID);
	}

	[Preserve]
	void Haptic(Haptic.Type _HapticType)
	{
		m_HapticProcessor.Process(_HapticType);
	}

	void ProcessScore()
	{
		if (m_ScoreLabel != null)
			m_ScoreLabel.text = ((long)Math.Round(m_Score * m_ScorePhase)).ToString();
	}

	void ProcessAccuracy()
	{
		if (m_AccuracyLabel != null)
			m_AccuracyLabel.text = Mathf.RoundToInt(m_Accuracy * m_AccuracyPhase).ToString();
	}

	void ProcessExpPayout()
	{
		if (m_PayoutLabel != null)
			m_PayoutLabel.Coins = (long)(m_ExpPayout * (double)m_ExpPayoutPhase);
	}

	void InvokeFinished()
	{
		Action action = m_Finished;
		m_Finished = null;
		action?.Invoke();
	}
}