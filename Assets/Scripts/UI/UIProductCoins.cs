using UnityEngine;
using Zenject;

public class UIProductCoins : UIProductEntity
{
	[SerializeField] UIUnitLabel m_Coins;

	[Inject] VouchersManager m_VouchersManager;

	protected override void Subscribe()
	{
		ProductsManager.Profile.Subscribe(DataEventType.Add, ProductID, ProcessData);
		ProductsManager.Collection.Subscribe(DataEventType.Change, ProductID, ProcessData);
		ProductsManager.Vouchers.SubscribeStart(ProcessData);
		ProductsManager.Vouchers.SubscribeCancel(ProcessData);
		ProductsManager.Vouchers.SubscribeEnd(ProcessData);
		ProductsManager.Vouchers.Profile.Subscribe(DataEventType.Add, ProcessData);
		ProductsManager.Vouchers.Profile.Subscribe(DataEventType.Remove, ProcessData);
		ProductsManager.Vouchers.Profile.Subscribe(DataEventType.Change, ProcessData);
		ProductsManager.Vouchers.Collection.Subscribe(DataEventType.Change, ProcessData);
	}

	protected override void Unsubscribe()
	{
		ProductsManager.Profile.Subscribe(DataEventType.Add, ProductID, ProcessData);
		ProductsManager.Collection.Subscribe(DataEventType.Change, ProductID, ProcessData);
		ProductsManager.Vouchers.SubscribeStart(ProcessData);
		ProductsManager.Vouchers.SubscribeCancel(ProcessData);
		ProductsManager.Vouchers.SubscribeEnd(ProcessData);
		ProductsManager.Vouchers.Profile.Subscribe(DataEventType.Add, ProcessData);
		ProductsManager.Vouchers.Profile.Subscribe(DataEventType.Remove, ProcessData);
		ProductsManager.Vouchers.Profile.Subscribe(DataEventType.Change, ProcessData);
		ProductsManager.Vouchers.Collection.Subscribe(DataEventType.Change, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Coins.Value = m_VouchersManager.GetProductDiscount(ProductID);
	}
}
