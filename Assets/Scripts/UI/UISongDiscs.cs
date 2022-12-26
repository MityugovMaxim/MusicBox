using UnityEngine;
using Zenject;

public class UISongDiscs : UISongEntity
{
	[SerializeField] GameObject m_Content;

	[SerializeField] GameObject m_BronzeDisc;
	[SerializeField] GameObject m_SilverDisc;
	[SerializeField] GameObject m_GoldDisc;
	[SerializeField] GameObject m_PlatinumDisc;

	[Inject] ScoresManager m_ScoresManager;

	protected override void Subscribe()
	{
		m_ScoresManager.Profile.Subscribe(DataEventType.Add, SongID, ProcessData);
		m_ScoresManager.Profile.Subscribe(DataEventType.Remove, SongID, ProcessData);
		m_ScoresManager.Profile.Subscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		m_ScoresManager.Profile.Unsubscribe(DataEventType.Add, SongID, ProcessData);
		m_ScoresManager.Profile.Unsubscribe(DataEventType.Remove, SongID, ProcessData);
		m_ScoresManager.Profile.Unsubscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void ProcessData()
	{
		RankType rank = m_ScoresManager.GetRank(SongID);
		
		m_Content.SetActive(rank > RankType.None);
		
		m_PlatinumDisc.SetActive(rank >= RankType.Platinum);
		m_GoldDisc.SetActive(rank >= RankType.Gold);
		m_SilverDisc.SetActive(rank >= RankType.Silver);
		m_BronzeDisc.SetActive(rank >= RankType.Bronze);
	}
}
