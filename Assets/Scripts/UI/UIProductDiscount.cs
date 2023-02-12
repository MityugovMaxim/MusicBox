using UnityEngine;
using Zenject;

public class UIProductDiscount : UIProductEntity
{
	[SerializeField] GameObject  m_Content;
	[SerializeField] UIUnitLabel m_Discount;

	[Inject] VouchersManager m_VouchersManager;

	protected override void Subscribe()
	{
		ProductsManager.Profile.Subscribe(DataEventType.Add, ProcessData);
		ProductsManager.Profile.Subscribe(DataEventType.Remove, ProcessData);
		ProductsManager.Profile.Subscribe(DataEventType.Change, ProcessData);
		ProductsManager.Collection.Subscribe(DataEventType.Change, ProcessData);
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
		ProductsManager.Profile.Unsubscribe(DataEventType.Add, ProcessData);
		ProductsManager.Profile.Unsubscribe(DataEventType.Remove, ProcessData);
		ProductsManager.Profile.Unsubscribe(DataEventType.Change, ProcessData);
		ProductsManager.Collection.Unsubscribe(DataEventType.Change, ProcessData);
		m_VouchersManager.UnsubscribeStart(ProcessData);
		m_VouchersManager.UnsubscribeCancel(ProcessData);
		m_VouchersManager.UnsubscribeEnd(ProcessData);
		m_VouchersManager.Profile.Unsubscribe(DataEventType.Add, ProcessData);
		m_VouchersManager.Profile.Unsubscribe(DataEventType.Remove, ProcessData);
		m_VouchersManager.Profile.Unsubscribe(DataEventType.Change, ProcessData);
		m_VouchersManager.Collection.Unsubscribe(DataEventType.Change, ProcessData);
	}

	protected override void ProcessData()
	{
		long source = ProductsManager.GetCoins(ProductID);
		long target = m_VouchersManager.GetProductDiscount(ProductID, source);
		
		m_Content.SetActive(source < target);
		m_Discount.Value = source;
	}
}
