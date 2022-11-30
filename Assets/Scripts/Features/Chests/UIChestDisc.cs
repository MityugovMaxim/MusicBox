using UnityEngine;

public class UIChestDisc : UIChestEntity
{
	[SerializeField] UIDisc m_Disc;

	protected override void Subscribe()
	{
		ChestsManager.Profile.Subscribe(DataEventType.Change, ChestID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		ChestsManager.Profile.Unsubscribe(DataEventType.Change, ChestID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Disc.Rank = ChestsManager.GetRank(ChestID);
	}
}
