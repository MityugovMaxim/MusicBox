using UnityEngine;
using Zenject;

public class UIOfferRewardChest : UIOfferReward
{
	protected override RewardType Type => RewardType.Chest;

	[SerializeField] UIChestImage m_Image;

	[Inject] ChestsManager m_ChestsManager;

	protected override void ProcessChest(string _ChestID)
	{
		RankType rank = m_ChestsManager.GetChestRank(_ChestID);
		
		SetActive(rank != RankType.None);
		
		m_Image.Rank = rank;
	}
}
