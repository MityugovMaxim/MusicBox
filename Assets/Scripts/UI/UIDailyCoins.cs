using UnityEngine;

public class UIDailyCoins : UIDailyEntity
{
	[SerializeField] UIUnitLabel m_Coins;

	public override void Subscribe()
	{
		DailyManager.Collection.Subscribe(DataEventType.Change, DailyID, ProcessData);
	}

	public override void Unsubscribe()
	{
		DailyManager.Collection.Unsubscribe(DataEventType.Change, DailyID, ProcessData);
	}

	public override void ProcessData()
	{
		m_Coins.Value = DailyManager.GetCoins(DailyID);
	}
}