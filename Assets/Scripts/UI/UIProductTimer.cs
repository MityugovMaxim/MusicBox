using UnityEngine;
using Zenject;

public class UIProductTimer : UIProductEntity
{
	[SerializeField] GameObject    m_Content;
	[SerializeField] UIAnalogTimer m_Timer;

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
		string voucherID = m_VouchersManager.GetProductVoucherID(ProductID);
		if (string.IsNullOrEmpty(voucherID))
		{
			m_Content.SetActive(false);
			return;
		}
		
		long timestamp      = TimeUtility.GetTimestamp();
		long startTimestamp = m_VouchersManager.GetStartTimestamp(voucherID);
		long endTimestamp   = m_VouchersManager.GetEndTimestamp(voucherID);
		long duration       = endTimestamp - startTimestamp;
		if (duration == 0 || timestamp < startTimestamp || timestamp > endTimestamp)
		{
			m_Content.SetActive(false);
			return;
		}
		
		m_Content.SetActive(true);
		
		m_Timer.SetTimer(endTimestamp);
	}
}
