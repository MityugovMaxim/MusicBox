using UnityEngine;

public class UIOfferRewardChest : UIOfferReward
{
	protected override RewardType Type => RewardType.Chest;

	[SerializeField] UIChestItem m_Image;

	protected override void ProcessChest(string _ChestID)
	{
		SetActive(!string.IsNullOrEmpty(_ChestID));
		
		m_Image.Setup(_ChestID);
	}
}
