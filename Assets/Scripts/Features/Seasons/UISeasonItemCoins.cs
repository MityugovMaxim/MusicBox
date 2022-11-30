using UnityEngine;

public class UISeasonItemCoins : UISeasonItemEntity
{
	[SerializeField] GameObject  m_Content;
	[SerializeField] UIUnitLabel m_Coins;

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
		long coins = SeasonsManager.GetCoins(SeasonID, ItemID);
		
		m_Content.SetActive(coins > 0);
		m_Coins.Value = coins;
	}
}
