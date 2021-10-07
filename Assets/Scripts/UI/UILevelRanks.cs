using UnityEngine;
using Zenject;

public class UILevelRanks : UIEntity
{
	[SerializeField] GameObject m_BronzeRank;
	[SerializeField] GameObject m_SilverRank;
	[SerializeField] GameObject m_GoldRank;
	[SerializeField] GameObject m_PlatinumRank;

	ScoreProcessor m_ScoreProcessor;

	[Inject]
	public void Construct(ScoreProcessor _ScoreProcessor)
	{
		m_ScoreProcessor = _ScoreProcessor;
	}

	public void Setup(string _LevelID)
	{
		ScoreRank rank = m_ScoreProcessor.GetRank(_LevelID);
		gameObject.SetActive(rank >= ScoreRank.None);
		m_PlatinumRank.SetActive(rank >= ScoreRank.Platinum);
		m_GoldRank.SetActive(rank >= ScoreRank.Gold);
		m_SilverRank.SetActive(rank >= ScoreRank.Silver);
		m_BronzeRank.SetActive(rank >= ScoreRank.Bronze);
	}
}
