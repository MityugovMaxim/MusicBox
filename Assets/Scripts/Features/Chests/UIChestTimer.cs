using UnityEngine;

public class UIChestTimer : UISlotEntity
{
	[SerializeField] UIAnalogTimer m_Timer;
	[SerializeField] UIGroup       m_TimerGroup;

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
		
		if (state == ChestSlotState.Processing)
			m_TimerGroup.Show();
		else
			m_TimerGroup.Hide();
		
		m_Timer.SetTimer(
			ChestsManager.GetSlotStartTimestamp(Slot),
			ChestsManager.GetSlotEndTimestamp(Slot)
		);
	}
}
