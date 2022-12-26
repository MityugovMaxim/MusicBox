using UnityEngine;

public class UIOfferRewardCoins : UIOfferReward
{
	protected override RewardType Type => RewardType.Coins;

	[SerializeField] UICoinsItem m_Coins;

	protected override void ProcessCoins(long _Coins)
	{
		SetActive(_Coins > 0);
		
		m_Coins.Setup(_Coins);
	}
}
