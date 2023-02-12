using UnityEngine;

public class UIChestTime : UISlotEntity
{
	[SerializeField] UIAnalogTimer m_Time;
	[SerializeField] UIGroup       m_TimeGroup;

	protected override void Subscribe()
	{
		ChestsManager.SubscribeStartTimer(Slot, ProcessData);
		ChestsManager.SubscribeEndTimer(Slot, ProcessData);
		ChestsManager.SubscribeCancelTimer(Slot, ProcessData);
		ChestsManager.Slots.Subscribe(DataEventType.Add, Slot, ProcessData);
		ChestsManager.Slots.Subscribe(DataEventType.Remove, Slot, ProcessData);
		ChestsManager.Slots.Subscribe(DataEventType.Change, Slot, ProcessData);
		ChestsManager.Collection.Subscribe(DataEventType.Change, ProcessData);
	}

	protected override void Unsubscribe()
	{
		ChestsManager.UnsubscribeStartTimer(Slot, ProcessData);
		ChestsManager.UnsubscribeEndTimer(Slot, ProcessData);
		ChestsManager.UnsubscribeCancelTimer(Slot, ProcessData);
		ChestsManager.Slots.Unsubscribe(DataEventType.Add, Slot, ProcessData);
		ChestsManager.Slots.Unsubscribe(DataEventType.Remove, Slot, ProcessData);
		ChestsManager.Slots.Unsubscribe(DataEventType.Change, Slot, ProcessData);
		ChestsManager.Collection.Unsubscribe(DataEventType.Change, ProcessData);
	}

	protected override void ProcessData()
	{
		ChestSlotState state = ChestsManager.GetSlotState(Slot);
		
		if (state == ChestSlotState.Pending)
			m_TimeGroup.Show();
		else
			m_TimeGroup.Hide();
		
		m_Time.SetTime(
			ChestsManager.GetSlotStartTimestamp(Slot),
			ChestsManager.GetSlotEndTimestamp(Slot)
		);
	}
}
