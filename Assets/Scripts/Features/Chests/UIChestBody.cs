using UnityEngine;

public class UIChestBody : UIChestEntity
{
	[SerializeField] Renderer m_Renderer;
	[SerializeField] Material m_BronzeChest;
	[SerializeField] Material m_SilverChest;
	[SerializeField] Material m_GoldChest;
	[SerializeField] Material m_PlatinumChest;

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
		switch (ChestsManager.GetType(ChestID))
		{
			case ChestType.Bronze:
				m_Renderer.material = m_BronzeChest;
				break;
			case ChestType.Silver:
				m_Renderer.material = m_SilverChest;
				break;
			case ChestType.Gold:
				m_Renderer.material = m_GoldChest;
				break;
			case ChestType.Platinum:
				m_Renderer.material = m_PlatinumChest;
				break;
		}
	}
}
