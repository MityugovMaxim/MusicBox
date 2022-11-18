using UnityEngine;
using Zenject;

public class UISongDiscs : UIEntity
{
	public string SongID
	{
		get => m_SongID;
		set
		{
			if (m_SongID == value)
				return;
			
			m_ScoresManager.Unsubscribe(DataEventType.Add, m_SongID, ProcessDiscs);
			m_ScoresManager.Unsubscribe(DataEventType.Remove, m_SongID, ProcessDiscs);
			m_ScoresManager.Unsubscribe(DataEventType.Change, m_SongID, ProcessDiscs);
			
			m_SongID = value;
			
			m_ScoresManager.Subscribe(DataEventType.Add, m_SongID, ProcessDiscs);
			m_ScoresManager.Subscribe(DataEventType.Remove, m_SongID, ProcessDiscs);
			m_ScoresManager.Subscribe(DataEventType.Change, m_SongID, ProcessDiscs);
			
			ProcessDiscs();
		}
	}

	[SerializeField] GameObject m_BronzeRank;
	[SerializeField] GameObject m_SilverRank;
	[SerializeField] GameObject m_GoldRank;
	[SerializeField] GameObject m_PlatinumRank;

	[Inject] ScoresManager m_ScoresManager;

	string m_SongID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		SongID = null;
	}

	void ProcessDiscs()
	{
		ScoreRank rank = m_ScoresManager.GetRank(SongID);
		
		gameObject.SetActive(rank >= ScoreRank.None);
		
		m_PlatinumRank.SetActive(rank >= ScoreRank.Platinum);
		m_GoldRank.SetActive(rank >= ScoreRank.Gold);
		m_SilverRank.SetActive(rank >= ScoreRank.Silver);
		m_BronzeRank.SetActive(rank >= ScoreRank.Bronze);
	}
}
