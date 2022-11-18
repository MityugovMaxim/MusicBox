using UnityEngine;
using Zenject;

public class UIProductCoins : UIEntity
{
	public string ProductID
	{
		get => m_ProductID;
		set
		{
			if (m_ProductID == value)
				return;
			
			m_VouchersManager.Unsubscribe(DataEventType.Add, ProcessCoins);
			m_VouchersManager.Unsubscribe(DataEventType.Remove, ProcessCoins);
			m_VouchersManager.Unsubscribe(DataEventType.Change, ProcessCoins);
			m_ProductsManager.Collection.Unsubscribe(DataEventType.Change, m_ProductID, ProcessCoins);
			
			m_ProductID = value;
			
			m_VouchersManager.Subscribe(DataEventType.Add, ProcessCoins);
			m_VouchersManager.Subscribe(DataEventType.Remove, ProcessCoins);
			m_VouchersManager.Subscribe(DataEventType.Change, ProcessCoins);
			m_ProductsManager.Collection.Subscribe(DataEventType.Change, m_ProductID, ProcessCoins);
			
			ProcessCoins();
		}
	}

	[SerializeField] UIUnitLabel m_Coins;

	[Inject] VouchersManager m_VouchersManager;
	[Inject] ProductsManager m_ProductsManager;

	string m_ProductID;

	void ProcessCoins()
	{
		m_Coins.Value = m_VouchersManager.GetProductDiscount(ProductID);
	}
}
