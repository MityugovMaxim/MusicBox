using UnityEngine;
using Zenject;

public class UISeasonItemChest : UISeasonItemEntity
{
	[SerializeField] GameObject   m_Content;
	[SerializeField] UIChestImage m_Image;

	[Inject] ChestsManager m_ChestsManager;

	protected override void Subscribe()
	{
		SeasonsManager.Collection.Subscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SeasonsManager.Collection.Unsubscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void ProcessData()
	{
		string chestID = SeasonsManager.GetChestID(SeasonID, Level, Mode);
		
		RankType rank = m_ChestsManager.GetChestRank(chestID);
		
		m_Content.SetActive(rank != RankType.None);
		
		m_Image.Rank = rank;
	}
}
