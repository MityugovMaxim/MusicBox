using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class UIProductPromo : UIGroupLayout, IPointerClickHandler
{
	[SerializeField] UIProductLabel     m_Label;
	[SerializeField] UIProductPrice     m_Price;
	[SerializeField] UIProductThumbnail m_Thumbnail;

	ProfileProcessor   m_ProfileProcessor;
	HapticProcessor    m_HapticProcessor;
	MenuProcessor      m_MenuProcessor;
	StatisticProcessor m_StatisticProcessor;

	string m_ProductID;

	[Inject]
	public void Construct(
		ProfileProcessor   _ProfileProcessor,
		MenuProcessor      _MenuProcessor,
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_ProfileProcessor   = _ProfileProcessor;
		m_MenuProcessor      = _MenuProcessor;
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
	}

	public async void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		if (string.IsNullOrEmpty(m_ProductID) || m_ProfileProcessor.HasProduct(m_ProductID))
		{
			await HideAsync();
			return;
		}
		
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
		m_Thumbnail.Setup(m_ProductID);
		
		await ShowAsync();
	}

	async void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		m_StatisticProcessor.LogMainMenuPromoClick(m_ProductID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactHeavy);
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		if (productMenu != null)
			productMenu.Setup(m_ProductID);
		
		await m_MenuProcessor.Show(MenuType.ProductMenu);
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Store, true);
	}
}