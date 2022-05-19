using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Zenject;

public class UIProductPromo : UIEntity, IPointerClickHandler
{
	[Preserve]
	public class Pool : UIEntityPool<UIProductPromo> { }

	[SerializeField] UIProductImage m_Image;
	[SerializeField] UIProductLabel m_Label;
	[SerializeField] UIProductPrice m_Price;

	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	string m_ProductID;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Image.Setup(m_ProductID);
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
	}

	async void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		m_StatisticProcessor.LogMainMenuPromoClick(m_ProductID);
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		if (productMenu != null)
			productMenu.Setup(m_ProductID);
		
		await m_MenuProcessor.Show(MenuType.ProductMenu);
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Store, true);
	}
}