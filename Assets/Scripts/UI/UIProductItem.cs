using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Zenject;

public class UIProductItem : UIEntity, IPointerClickHandler
{
	[Preserve]
	public class Pool : UIEntityPool<UIProductItem> { }

	[SerializeField] UIProductImage    m_Image;
	[SerializeField] UIProductDiscount m_Discount;
	[SerializeField] UIProductPrice    m_Price;
	[SerializeField] UIUnitLabel       m_Coins;

	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;
	[Inject] ProductsProcessor  m_ProductsProcessor;

	string m_ProductID;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Image.Setup(m_ProductID);
		m_Discount.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
		
		m_Coins.Value = m_ProductsProcessor.GetCoins(m_ProductID);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		m_StatisticProcessor.LogMainMenuStorePageItemClick(m_ProductID);
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		
		if (productMenu == null)
			return;
		
		productMenu.Setup(m_ProductID);
		productMenu.Show();
	}
}