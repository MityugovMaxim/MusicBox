using UnityEngine;

public class UISeasonItemState : UISeasonItemEntity
{
	[SerializeField] GameObject m_Collect;
	[SerializeField] GameObject m_Purchase;

	protected override void Subscribe()
	{
		SeasonsManager.SubscribePass(SeasonID, ProcessData);
		SeasonsManager.Profile.Subscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SeasonsManager.UnsubscribePass(SeasonID, ProcessData);
		SeasonsManager.Profile.Unsubscribe(DataEventType.Change, SeasonID, ProcessData);
	}

	protected override void ProcessData()
	{
		bool available = SeasonsManager.IsItemAvailable(SeasonID, ItemID);
		bool pass      = SeasonsManager.HasPass(SeasonID);
		bool free      = SeasonsManager.IsFreeItem(SeasonID, ItemID);
		bool paid      = SeasonsManager.IsFreeItem(SeasonID, ItemID);
		
		m_Collect.SetActive(available && free || available && paid && pass);
		
		m_Purchase.SetActive(available && paid && !pass);
	}
}