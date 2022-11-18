using UnityEngine;
using Zenject;

public class UIProductDiscount : UIEntity
{
	public string ProductID
	{
		get => m_ProductID;
		set
		{
			if (m_ProductID == value)
				return;
			
			m_VouchersManager.Unsubscribe(DataEventType.Add, ProcessDiscount);
			m_VouchersManager.Unsubscribe(DataEventType.Remove, ProcessDiscount);
			m_VouchersManager.Unsubscribe(DataEventType.Change, ProcessDiscount);
			m_ProductsManager.Collection.Unsubscribe(DataEventType.Change, m_ProductID, ProcessDiscount);
			
			m_ProductID = value;
			
			m_VouchersManager.Subscribe(DataEventType.Add, ProcessDiscount);
			m_VouchersManager.Subscribe(DataEventType.Remove, ProcessDiscount);
			m_VouchersManager.Subscribe(DataEventType.Change, ProcessDiscount);
			m_ProductsManager.Collection.Subscribe(DataEventType.Change, m_ProductID, ProcessDiscount);
			
			ProcessDiscount();
		}
	}

	[SerializeField] GameObject  m_Content;
	[SerializeField] UIUnitLabel m_Discount;

	[Inject] ProductsManager m_ProductsManager;
	[Inject] VouchersManager m_VouchersManager;

	string m_ProductID;

	void ProcessDiscount()
	{
		string voucherID = m_VouchersManager.GetProductVoucherID(ProductID);
		
		if (string.IsNullOrEmpty(voucherID))
		{
			m_Content.SetActive(false);
			return;
		}
		
		long source = m_ProductsManager.GetCoins(ProductID);
		long target = m_VouchersManager.GetProductDiscount(ProductID);
		
		if (source == target)
		{
			m_Content.SetActive(false);
			return;
		}
		
		m_Discount.Value = source;
		
		m_Content.SetActive(true);
	}
}
