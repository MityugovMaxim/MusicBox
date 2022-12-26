using UnityEngine;

public class UIChestBoost : UIChestEntity
{
	[SerializeField] UIUnitLabel m_Coins;
	[SerializeField] UIGroup     m_CoinsGroup;

	protected override void Subscribe()
	{
		ChestsInventory.SubscribeStart(ChestID, ProcessData);
		ChestsInventory.SubscribeCancel(ChestID, ProcessData);
		ChestsInventory.SubscribeEnd(ChestID, ProcessData);
		ChestsManager.Collection.Subscribe(DataEventType.Change, ChestID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		ChestsInventory.UnsubscribeStart(ChestID, ProcessData);
		ChestsInventory.UnsubscribeCancel(ChestID, ProcessData);
		ChestsInventory.UnsubscribeEnd(ChestID, ProcessData);
		ChestsManager.Collection.Unsubscribe(DataEventType.Change, ChestID, ProcessData);
	}

	protected override void ProcessData()
	{
		RankType rank = ChestsInventory.GetRank(ChestID);
		
		m_Coins.Value = ChestsManager.GetBoost(rank);
		
		if (ChestsInventory.IsProcessing(ChestID))
			m_CoinsGroup.Show();
		else
			m_CoinsGroup.Hide();
	}
}
