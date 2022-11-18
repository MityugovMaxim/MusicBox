using UnityEngine;
using Zenject;

public class UIProductTimer : UIEntity
{
	public string ProductID
	{
		get => m_ProductID;
		set
		{
			if (m_ProductID == value)
				return;
			
			string voucherID = m_VouchersManager.GetProductVoucherID(m_ProductID);
			
			m_VouchersManager.Unsubscribe(DataEventType.Add, ProcessTimer);
			m_VouchersManager.Unsubscribe(DataEventType.Remove, ProcessTimer);
			m_VouchersManager.Unsubscribe(DataEventType.Change, voucherID, ProcessTimer);
			m_VouchersManager.UnsubscribeExpiration(voucherID, ProcessTimer);
			
			m_ProductID = value;
			
			m_VouchersManager.Subscribe(DataEventType.Add, ProcessTimer);
			m_VouchersManager.Subscribe(DataEventType.Remove, ProcessTimer);
			m_VouchersManager.Subscribe(DataEventType.Change, voucherID, ProcessTimer);
			m_VouchersManager.SubscribeExpiration(voucherID, ProcessTimer);
		}
	}

	[SerializeField] UIAnalogTimer m_Timer;

	[Inject] VouchersManager m_VouchersManager;

	string m_ProductID;

	void ProcessTimer()
	{
		string voucherID = m_VouchersManager.GetProductVoucherID(m_ProductID);
		
		if (string.IsNullOrEmpty(voucherID))
		{
			gameObject.SetActive(false);
			return;
		}
		
		long timestamp  = TimeUtility.GetTimestamp();
		long expiration = m_VouchersManager.GetExpiration(voucherID);
		
		if (expiration == 0 || expiration < timestamp)
		{
			gameObject.SetActive(false);
			return;
		}
		
		m_Timer.Setup(expiration);
		
		gameObject.SetActive(true);
	}
}
