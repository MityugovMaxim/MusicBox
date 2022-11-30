using UnityEngine;

public class UIProductTimer : UIProductEntity
{
	[SerializeField] UIAnalogTimer m_Timer;

	protected override void Subscribe()
	{
		ProductsManager.Profile.Subscribe(DataEventType.Add, ProcessData);
		ProductsManager.Profile.Subscribe(DataEventType.Remove, ProcessData);
		ProductsManager.Profile.Subscribe(DataEventType.Change, ProcessData);
		ProductsManager.Collection.Subscribe(DataEventType.Change, ProcessData);
		ProductsManager.Vouchers.SubscribeCancel(ProcessData);
		ProductsManager.Vouchers.SubscribeExpiration(ProcessData);
		ProductsManager.Vouchers.Profile.Subscribe(DataEventType.Add, ProcessData);
		ProductsManager.Vouchers.Profile.Subscribe(DataEventType.Remove, ProcessData);
		ProductsManager.Vouchers.Profile.Subscribe(DataEventType.Change, ProcessData);
		ProductsManager.Vouchers.Collection.Subscribe(DataEventType.Change, ProcessData);
	}

	protected override void Unsubscribe()
	{
		ProductsManager.Profile.Unsubscribe(DataEventType.Add, ProcessData);
		ProductsManager.Profile.Unsubscribe(DataEventType.Remove, ProcessData);
		ProductsManager.Profile.Unsubscribe(DataEventType.Change, ProcessData);
		ProductsManager.Collection.Unsubscribe(DataEventType.Change, ProcessData);
		ProductsManager.Vouchers.UnsubscribeCancel(ProcessData);
		ProductsManager.Vouchers.UnsubscribeExpiration(ProcessData);
		ProductsManager.Vouchers.Profile.Unsubscribe(DataEventType.Add, ProcessData);
		ProductsManager.Vouchers.Profile.Unsubscribe(DataEventType.Remove, ProcessData);
		ProductsManager.Vouchers.Profile.Unsubscribe(DataEventType.Change, ProcessData);
		ProductsManager.Vouchers.Collection.Unsubscribe(DataEventType.Change, ProcessData);
	}

	protected override void ProcessData()
	{
		string voucherID = ProductsManager.GetVoucherID(ProductID);
		
		if (string.IsNullOrEmpty(voucherID))
		{
			gameObject.SetActive(false);
			return;
		}
		
		long timestamp  = TimeUtility.GetTimestamp();
		long expiration = ProductsManager.Vouchers.GetExpiration(voucherID);
		
		if (expiration == 0 || expiration < timestamp)
		{
			gameObject.SetActive(false);
			return;
		}
		
		m_Timer.Setup(expiration);
		
		gameObject.SetActive(true);
	}
}
