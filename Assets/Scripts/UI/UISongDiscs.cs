using UnityEngine;
using Zenject;

public class UISongDiscs : UIEntity
{
	[SerializeField] GameObject m_BronzeRank;
	[SerializeField] GameObject m_SilverRank;
	[SerializeField] GameObject m_GoldRank;
	[SerializeField] GameObject m_PlatinumRank;

	[Inject] ScoreProcessor m_ScoreProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		ScoreRank rank = m_ScoreProcessor.GetRank(m_SongID);
		
		gameObject.SetActive(rank >= ScoreRank.None);
		
		m_PlatinumRank.SetActive(rank >= ScoreRank.Platinum);
		m_GoldRank.SetActive(rank >= ScoreRank.Gold);
		m_SilverRank.SetActive(rank >= ScoreRank.Silver);
		m_BronzeRank.SetActive(rank >= ScoreRank.Bronze);
	}
}
