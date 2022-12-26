using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIProductCoinsElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIProductCoinsElement> { }

	[SerializeField] UIProductImage    m_Image;
	[SerializeField] UIProductPrice    m_Price;
	[SerializeField] UIProductCoins    m_Coins;
	[SerializeField] UIProductDiscount m_Discount;
	[SerializeField] UIProductTimer    m_Timer;
	[SerializeField] UIProductAction   m_Action;

	[Inject] BadgeManager m_BadgeManager;

	public void Setup(string _ProductID)
	{
		m_Image.ProductID    = _ProductID;
		m_Price.ProductID    = _ProductID;
		m_Coins.ProductID    = _ProductID;
		m_Discount.ProductID = _ProductID;
		m_Timer.ProductID    = _ProductID;
		m_Action.ProductID   = _ProductID;
		
		m_BadgeManager.ReadProduct(_ProductID);
	}
}
