using UnityEngine;

public class UIChestBoost : UISlotEntity
{
	[SerializeField] UIUnitLabel m_Coins;
	[SerializeField] UIGroup     m_CoinsGroup;

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
		ChestsManager.Collection.Subscribe(DataEventType.Change, ProcessData);
	}

	protected override void ProcessData()
	{
		RankType       rank  = ChestsManager.GetSlotRank(Slot);
		ChestSlotState state = ChestsManager.GetSlotState(Slot);
		
		m_Coins.Value = ChestsManager.GetChestBoost(rank);
		
		if (state == ChestSlotState.Processing || state == ChestSlotState.Pending)
			m_CoinsGroup.Show();
		else
			m_CoinsGroup.Hide();
	}
}
