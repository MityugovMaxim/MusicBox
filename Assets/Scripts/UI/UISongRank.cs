using UnityEngine;

public class UISongRank : UISongEntity
{
	[SerializeField] GameObject m_BronzeRank;
	[SerializeField] GameObject m_SilverRank;
	[SerializeField] GameObject m_GoldRank;
	[SerializeField] GameObject m_PlatinumRank;

	protected override void Subscribe()
	{
		SongsManager.Collection.Subscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SongsManager.Collection.Unsubscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void ProcessData()
	{
		RankType songRank = SongsManager.GetRank(SongID);
		
		m_BronzeRank.SetActive(songRank == RankType.Bronze);
		m_SilverRank.SetActive(songRank == RankType.Silver);
		m_GoldRank.SetActive(songRank == RankType.Gold);
		m_PlatinumRank.SetActive(songRank == RankType.Platinum);
	}
}
