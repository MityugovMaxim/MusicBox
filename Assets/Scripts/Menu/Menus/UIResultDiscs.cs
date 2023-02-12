using System;
using UnityEngine;
using Zenject;

public class UIResultDiscs : UIGroup
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

	[SerializeField] UIResultDisc m_Bronze;
	[SerializeField] UIResultDisc m_Silver;
	[SerializeField] UIResultDisc m_Gold;
	[SerializeField] UIResultDisc m_Platinum;

	[Inject] SongsManager      m_SongsManager;
	[Inject] ScoresManager     m_ScoresManager;
	[Inject] DifficultyManager m_DifficultyManager;

	string   m_SongID;
	RankType m_SongRank;
	RankType m_ScoreRank;
	int      m_Accuracy;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_SongRank  = m_SongsManager.GetRank(m_SongID);
		m_ScoreRank = m_ScoresManager.GetRank(m_SongID);
	}

	protected override void OnShowStarted()
	{
		foreach (RankType rank in Enum.GetValues(typeof(RankType)))
		{
			UIResultDisc disc = GetDisc(rank);
			
			if (disc == null)
				continue;
			
			disc.Restore();
			
			if (rank <= m_ScoreRank)
				disc.Activate();
			else
				disc.Deactivate();
		}
	}

	void ProcessAccuracy()
	{
		RankType scoreRank = m_DifficultyManager.GetRank(m_SongRank, Accuracy);
		
		if (m_ScoreRank >= scoreRank)
			return;
		
		m_ScoreRank = scoreRank;
		
		UIResultDisc disc = GetDisc(m_ScoreRank);
		
		if (disc == null)
			return;
		
		disc.Claim();
	}

	UIResultDisc GetDisc(RankType _Rank)
	{
		switch (_Rank)
		{
			case RankType.Bronze:   return m_Bronze;
			case RankType.Silver:   return m_Silver;
			case RankType.Gold:     return m_Gold;
			case RankType.Platinum: return m_Platinum;
			default:                return null;
		}
	}
}
