using UnityEngine;

public class UIDailyAds : UIDailyEntity
{
	[SerializeField] GameObject m_Ads;

	protected override void Subscribe()
	{
		DailyManager.Collection.Subscribe(DataEventType.Change, DailyID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		DailyManager.Collection.Unsubscribe(DataEventType.Change, DailyID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Ads.SetActive(DailyManager.IsAds(DailyID));
	}
}
