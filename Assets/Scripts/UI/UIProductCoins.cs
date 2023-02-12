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
		m_VouchersManager.SubscribeStart(ProcessData);
		m_VouchersManager.SubscribeCancel(ProcessData);
		m_VouchersManager.SubscribeEnd(ProcessData);
		m_VouchersManager.Profile.Subscribe(DataEventType.Add, ProcessData);
		m_VouchersManager.Profile.Subscribe(DataEventType.Remove, ProcessData);
		m_VouchersManager.Profile.Subscribe(DataEventType.Change, ProcessData);
		m_VouchersManager.Collection.Subscribe(DataEventType.Change, ProcessData);
	}

	protected override void Unsubscribe()
	{
		ProductsManager.Profile.Subscribe(DataEventType.Add, ProductID, ProcessData);
		ProductsManager.Collection.Subscribe(DataEventType.Change, ProductID, ProcessData);
		m_VouchersManager.SubscribeStart(ProcessData);
		m_VouchersManager.SubscribeCancel(ProcessData);
		m_VouchersManager.SubscribeEnd(ProcessData);
		m_VouchersManager.Profile.Subscribe(DataEventType.Add, ProcessData);
		m_VouchersManager.Profile.Subscribe(DataEventType.Remove, ProcessData);
		m_VouchersManager.Profile.Subscribe(DataEventType.Change, ProcessData);
		m_VouchersManager.Collection.Subscribe(DataEventType.Change, ProcessData);
	}

	protected override void ProcessData()
	{
		long coins = ProductsManager.GetCoins(ProductID);
		
		m_Coins.Value = m_VouchersManager.GetProductDiscount(ProductID, coins);
	}
}
