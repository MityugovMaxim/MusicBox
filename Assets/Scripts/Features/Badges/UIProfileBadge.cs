using System.Collections.Generic;
using System.Linq;
using Zenject;

public class UIProfileBadge : UIBadge
{
	[Inject] VouchersManager m_VouchersManager;

	protected override void Subscribe()
	{
		m_VouchersManager.SubscribeStart(Process);
		m_VouchersManager.SubscribeEnd(Process);
		m_VouchersManager.SubscribeCancel(Process);
	}

	protected override void Unsubscribe()
	{
		m_VouchersManager.UnsubscribeStart(Process);
		m_VouchersManager.UnsubscribeEnd(Process);
		m_VouchersManager.UnsubscribeCancel(Process);
	}

	protected override void Process()
	{
		int value = 0;
		
		value += GetVouchersCount();
		
		SetValue(value);
	}

	int GetVouchersCount()
	{
		List<string> voucherIDs = m_VouchersManager.GetVoucherIDs();
		
		return voucherIDs?.Count(_VoucherID => BadgeManager.IsVoucherUnread(_VoucherID)) ?? 0;
	}
}