using UnityEngine;
using Zenject;

public class UIProductCoins : UIEntity
{
	[SerializeField] UIUnitLabel m_Coins;

	[Inject] ProductsProcessor m_ProductsProcessor;
	[Inject] VouchersProcessor m_VouchersProcessor;

	string m_ProductID;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		long coins = m_ProductsProcessor.GetCoins(m_ProductID);
		
		m_Coins.Value = m_VouchersProcessor.GetValue(VoucherType.ProductDiscount, m_ProductID, coins);
	}
}