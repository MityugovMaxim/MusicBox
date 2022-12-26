using UnityEngine;

public class UIChestTimer : UIChestEntity
{
	[SerializeField] UIAnalogTimer m_Timer;
	[SerializeField] UIGroup       m_TimerGroup;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_TimerGroup.Hide(true);
	}

	protected override void Subscribe()
	{
		ChestsInventory.SubscribeStart(ChestID, ProcessData);
		ChestsInventory.SubscribeEnd(ChestID, ProcessData);
		ChestsInventory.SubscribeCancel(ChestID, ProcessData);
		ChestsInventory.Profile.Subscribe(DataEventType.Change, ChestID, ProcessData);
		ChestsManager.Collection.Subscribe(DataEventType.Change, ProcessData);
	}

	protected override void Unsubscribe()
	{
		ChestsInventory.UnsubscribeStart(ChestID, ProcessData);
		ChestsInventory.UnsubscribeEnd(ChestID, ProcessData);
		ChestsInventory.UnsubscribeCancel(ChestID, ProcessData);
		ChestsInventory.Profile.Unsubscribe(DataEventType.Change, ChestID, ProcessData);
		ChestsManager.Collection.Unsubscribe(DataEventType.Change, ProcessData);
	}

	protected override void ProcessData()
	{
		if (ChestsInventory.IsProcessing(ChestID))
			m_TimerGroup.Show(true);
		else
			m_TimerGroup.Hide(true);
		
		m_Timer.SetTimer(
			ChestsInventory.GetStartTimestamp(ChestID),
			ChestsInventory.GetEndTimestamp(ChestID)
		);
	}
}
