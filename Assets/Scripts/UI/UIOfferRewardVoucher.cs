using UnityEngine;

public class UIOfferRewardVoucher : UIOfferReward
{
	protected override RewardType Type => RewardType.Voucher;

	[SerializeField] UIVoucherItem m_Voucher;

	protected override void ProcessVoucher(string _VoucherID)
	{
		SetActive(!string.IsNullOrEmpty(_VoucherID));
		
		m_Voucher.Setup(_VoucherID);
	}
}
