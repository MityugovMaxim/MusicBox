using UnityEngine;

public class UIChestImage : UIChestEntity
{
	[SerializeField] UIChestIcon m_Icon;

	protected override void Subscribe()
	{
		ChestsInventory.Profile.Subscribe(DataEventType.Change, ChestID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		ChestsInventory.Profile.Unsubscribe(DataEventType.Change, ChestID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Icon.ChestRank = ChestsInventory.GetRank(ChestID);
	}
}
