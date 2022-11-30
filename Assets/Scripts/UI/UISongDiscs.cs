using UnityEngine;
using Zenject;

public class UISongDiscs : UISongEntity
{
	[SerializeField] GameObject m_Content;

	[SerializeField] GameObject m_BronzeDisc;
	[SerializeField] GameObject m_SilverDisc;
	[SerializeField] GameObject m_GoldDisc;
	[SerializeField] GameObject m_PlatinumDisc;

	[SerializeField] GameObject m_BronzePlaceholder;
	[SerializeField] GameObject m_SilverPlaceholder;
	[SerializeField] GameObject m_GoldPlaceholder;
	[SerializeField] GameObject m_PlatinumPlaceholder;

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
		ScoreRank rank = m_ScoresManager.GetRank(SongID);
		
		m_Content.SetActive(rank > ScoreRank.None);
		
		m_PlatinumDisc.SetActive(rank >= ScoreRank.Platinum);
		m_GoldDisc.SetActive(rank >= ScoreRank.Gold);
		m_SilverDisc.SetActive(rank >= ScoreRank.Silver);
		m_BronzeDisc.SetActive(rank >= ScoreRank.Bronze);
		
		m_PlatinumPlaceholder.SetActive(rank < ScoreRank.Platinum);
		m_GoldPlaceholder.SetActive(rank < ScoreRank.Gold);
		m_SilverPlaceholder.SetActive(rank < ScoreRank.Silver);
		m_BronzePlaceholder.SetActive(rank < ScoreRank.Bronze);
	}
}
