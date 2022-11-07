using UnityEngine;
using Zenject;

public class UIProductDiscount : UIEntity
{
	[SerializeField] UIUnitLabel m_Discount;

	[Inject] ProductsProcessor m_ProductsProcessor;
	[Inject] VouchersProcessor m_VouchersProcessor;

	string m_ProductID;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		string voucherID = m_VouchersProcessor.GetVoucherID(VoucherType.ProductDiscount, m_ProductID);
		
		if (string.IsNullOrEmpty(voucherID))
		{
			gameObject.SetActive(false);
			return;
		}
		
		long source = m_ProductsProcessor.GetCoins(m_ProductID);
		long target = m_VouchersProcessor.GetValue(VoucherType.ProductDiscount, m_ProductID, source);
		
		if (source == target)
		{
			gameObject.SetActive(false);
			return;
		}
		
		m_Discount.Value = source;
		
		gameObject.SetActive(true);
	}
}