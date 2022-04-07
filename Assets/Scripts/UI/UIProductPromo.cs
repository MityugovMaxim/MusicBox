using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class UIProductPromo : UIGroupLayout, IPointerClickHandler
{
	[SerializeField] UIProductImage m_Image;
	[SerializeField] UIProductLabel m_Label;
	[SerializeField] UIProductPrice m_Price;

	[Inject] ProfileProcessor   m_ProfileProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	string m_ProductID;

	public async void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		if (string.IsNullOrEmpty(m_ProductID) || m_ProfileProcessor.HasProduct(m_ProductID))
		{
			await HideAsync();
			return;
		}
		
		m_Image.Setup(m_ProductID);
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
		
		await ShowAsync();
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