using UnityEngine;

public class UIVoucherTimer : UIVoucherEntity
{
	[SerializeField] UIAnalogTimer m_Timer;
	[SerializeField] GameObject    m_Content;

	protected override void Subscribe()
	{
		VouchersManager.SubscribeStart(VoucherID, ProcessData);
		VouchersManager.SubscribeCancel(VoucherID, ProcessData);
		VouchersManager.SubscribeEnd(VoucherID, ProcessData);
		VouchersManager.Collection.Subscribe(DataEventType.Change, VoucherID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		VouchersManager.UnsubscribeStart(VoucherID, ProcessData);
		VouchersManager.UnsubscribeCancel(VoucherID, ProcessData);
		VouchersManager.UnsubscribeEnd(VoucherID, ProcessData);
		VouchersManager.Collection.Unsubscribe(DataEventType.Change, VoucherID, ProcessData);
	}

	protected override void ProcessData()
	{
		long timestamp      = TimeUtility.GetTimestamp();
		long startTimestamp = VouchersManager.GetStartTimestamp(VoucherID);
		long expiration     = VouchersManager.GetEndTimestamp(VoucherID);
		
		m_Content.SetActive(timestamp < expiration);
		m_Timer.SetTimer(expiration);
	}
}
