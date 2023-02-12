using System.Collections;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIResultCoins : UIGroup
{
	public int Accuracy
	{
		get => m_Accuracy;
		set
		{
			if (m_Accuracy == value)
				return;
			
			m_Accuracy = value;
			
			ProcessAccuracy();
		}
	}

	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField] UIUnitLabel m_Coins;
	[SerializeField] UIUnitLabel m_Delta;
	[SerializeField] float       m_Duration = 0.2f;

	[Inject] DifficultyManager m_DifficultyManager;
	[Inject] SongsManager      m_SongsManager;
	[Inject] ScoresManager     m_ScoresManager;

	string      m_SongID;
	int         m_Accuracy;
	Animator    m_Animator;
	long        m_Value;
	RankType    m_SongRank;
	RankType    m_ScoreRank;
	IEnumerator m_CoinsRoutine;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_SongRank  = m_SongsManager.GetRank(m_SongID);
		m_ScoreRank = m_ScoresManager.GetRank(m_SongID);
	}

	public void Append(long _Coins) => ProcessCoins(m_Value + _Coins);

	protected override void OnShowStarted()
	{
		m_Animator.ResetTrigger(m_PlayParameterID);
		m_Animator.SetTrigger(m_RestoreParameterID);
		m_Animator.Update(0);
		
		m_Coins.Value = 0;
		m_Delta.Value = 0;
	}

	void ProcessAccuracy()
	{
		RankType scoreRank = m_DifficultyManager.GetRank(m_SongRank, Accuracy);
		
		long coins = m_DifficultyManager.GetPayout(m_SongRank, m_ScoreRank, scoreRank);
		
		ProcessCoins(coins);
	}

	void ProcessCoins(long _Coins)
	{
		if (m_Value == _Coins)
			return;
		
		long delta = _Coins - m_Value;
		
		m_Value = _Coins;
		
		if (m_CoinsRoutine != null)
		{
			StopCoroutine(m_CoinsRoutine);
			m_CoinsRoutine = null;
		}
		
		m_Delta.Value = delta;
		
		if (gameObject.activeInHierarchy && delta > 0)
		{
			m_CoinsRoutine = CoinsRoutine(_Coins);
			
			StartCoroutine(m_CoinsRoutine);
			
			m_Animator.SetTrigger(m_PlayParameterID);
		}
		else
		{
			m_Coins.Value = m_Value;
		}
	}

	IEnumerator CoinsRoutine(long _Coins)
	{
		long coins = (long)m_Coins.Value;
		
		float time = 0;
		while (time < m_Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			m_Coins.Value = MathUtility.Lerp(coins, _Coins, time / m_Duration);
		}
		
		m_Coins.Value = _Coins;
	}
}
