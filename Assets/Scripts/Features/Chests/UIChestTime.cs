using UnityEngine;

public class UIChestTime : UIChestEntity
{
	[SerializeField] UIAnalogTimer m_Time;
	[SerializeField] UIGroup       m_TimeGroup;

	protected override void Subscribe()
	{
		ChestsInventory.SubscribeStart(ChestID, ProcessData);
		ChestsInventory.SubscribeCancel(ChestID, ProcessData);
		ChestsInventory.SubscribeEnd(ChestID, ProcessData);
		ChestsInventory.Profile.Subscribe(DataEventType.Change, ChestID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		ChestsInventory.UnsubscribeStart(ChestID, ProcessData);
		ChestsInventory.UnsubscribeCancel(ChestID, ProcessData);
		ChestsInventory.UnsubscribeEnd(ChestID, ProcessData);
		ChestsInventory.Profile.Unsubscribe(DataEventType.Change, ChestID, ProcessData);
	}

	protected override void ProcessData()
	{
		if (!ChestsInventory.IsSelected(ChestID) || ChestsInventory.IsProcessing(ChestID) || ChestsInventory.IsReady(ChestID))
		{
			m_TimeGroup.Hide();
			return;
		}
		
		m_TimeGroup.Show();
		
		long startTimestamp = ChestsInventory.GetStartTimestamp(ChestID);
		long endTimestamp   = ChestsInventory.GetEndTimestamp(ChestID);
		
		m_Time.SetTime(startTimestamp, endTimestamp);
	}
}
