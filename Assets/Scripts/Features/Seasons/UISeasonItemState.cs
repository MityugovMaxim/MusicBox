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
		bool available = SeasonsManager.IsItemAvailable(SeasonID, Level, Mode);
		bool pass      = SeasonsManager.HasPass(SeasonID);
		
		m_Collect.SetActive(available && Mode == SeasonItemMode.Free || available && Mode == SeasonItemMode.Paid && pass);
		
		m_Purchase.SetActive(available && Mode == SeasonItemMode.Paid && !pass);
	}
}
