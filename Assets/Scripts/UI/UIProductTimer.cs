using UnityEngine;
using Zenject;

public class UIProductTimer : UIEntity
{
	[SerializeField] UIAnalogTimer m_Timer;

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
		
		long timestamp  = TimeUtility.GetTimestamp();
		long expiration = m_VouchersProcessor.GetExpiration(voucherID);
		
		if (expiration == 0 || expiration < timestamp)
		{
			gameObject.SetActive(false);
			return;
		}
		
		m_Timer.Setup(expiration);
		
		gameObject.SetActive(true);
	}
}
