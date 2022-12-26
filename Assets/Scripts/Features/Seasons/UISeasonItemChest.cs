using UnityEngine;

public class UISeasonItemChest : UISeasonItemEntity
{
	[SerializeField] GameObject  m_Content;
	[SerializeField] UIChestItem m_Chest;

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
		
		m_Content.SetActive(!string.IsNullOrEmpty(chestID));
		
		m_Chest.Setup(chestID);
	}
}
