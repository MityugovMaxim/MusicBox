using System.Collections;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIResultPoints : UIGroup
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

	[SerializeField] UIUnitLabel m_Points;
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
	IEnumerator m_PointsRoutine;

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

	public void Append(long _Coins) => ProcessPoints(m_Value + _Coins);

	protected override void OnShowStarted()
	{
		m_Animator.ResetTrigger(m_PlayParameterID);
		m_Animator.SetTrigger(m_RestoreParameterID);
		m_Animator.Update(0);
		
		m_Points.Value = 0;
		m_Delta.Value = 0;
	}

	void ProcessAccuracy()
	{
		RankType scoreRank = m_DifficultyManager.GetRank(m_SongRank, Accuracy);
		
		long points = m_DifficultyManager.GetPoints(m_SongRank, m_ScoreRank, scoreRank);
		
		ProcessPoints(points);
	}

	void ProcessPoints(long _Points)
	{
		if (m_Value == _Points)
			return;
		
		long delta = _Points - m_Value;
		
		m_Value = _Points;
		
		if (m_PointsRoutine != null)
		{
			StopCoroutine(m_PointsRoutine);
			m_PointsRoutine = null;
		}
		
		m_Delta.Value = delta;
		
		if (gameObject.activeInHierarchy && delta > 0)
		{
			m_PointsRoutine = PointsRoutine(_Points);
			
			StartCoroutine(m_PointsRoutine);
			
			m_Animator.SetTrigger(m_PlayParameterID);
		}
		else
		{
			m_Points.Value = m_Value;
		}
	}

	IEnumerator PointsRoutine(long _Points)
	{
		long points = (long)m_Points.Value;
		
		float time = 0;
		while (time < m_Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			m_Points.Value = MathUtility.Lerp(points, _Points, time / m_Duration);
		}
		
		m_Points.Value = _Points;
	}
}
