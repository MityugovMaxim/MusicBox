using UnityEngine;
using Zenject;

public class UIScoreRank : UIEntity
{
	public enum ScoreType
	{
		Last = 0,
		Best = 1,
	}

	[SerializeField] ScoreType  m_ScoreType;
	[SerializeField] GameObject m_RankS;
	[SerializeField] GameObject m_RankA;
	[SerializeField] GameObject m_RankB;
	[SerializeField] GameObject m_RankC;
	[SerializeField] GameObject m_Background;

	ScoreProcessor m_ScoreProcessor;

	[Inject]
	public void Construct(ScoreProcessor _ScoreProcessor)
	{
		m_ScoreProcessor = _ScoreProcessor;
	}

	public void Setup(string _LevelID)
	{
		ScoreRank rank = m_ScoreType == ScoreType.Last
			? m_ScoreProcessor.GetLastRank(_LevelID)
			: m_ScoreProcessor.GetBestRank(_LevelID);
		
		m_RankS.SetActive(false);
		m_RankA.SetActive(false);
		m_RankB.SetActive(false);
		m_RankC.SetActive(false);
		
		if (m_Background != null)
			m_Background.SetActive(rank != ScoreRank.None);
		
		switch (rank)
		{
			case ScoreRank.S:
				m_RankS.SetActive(true);
				break;
			case ScoreRank.A:
				m_RankA.SetActive(true);
				break;
			case ScoreRank.B:
				m_RankB.SetActive(true);
				break;
			case ScoreRank.C:
				m_RankC.SetActive(true);
				break;
		}
	}
}