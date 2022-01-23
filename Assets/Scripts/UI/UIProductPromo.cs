using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class UIProductPromo : UIGroupLayout, IPointerClickHandler
{
	[SerializeField] UIProductLabel     m_Label;
	[SerializeField] UIProductPrice     m_Price;
	[SerializeField] UIProductThumbnail m_Thumbnail;

	ProfileProcessor m_ProfileProcessor;
	HapticProcessor  m_HapticProcessor;
	MenuProcessor    m_MenuProcessor;

	string m_ProductID;

	[Inject]
	public void Construct(
		ProfileProcessor _ProfileProcessor,
		MenuProcessor    _MenuProcessor,
		HapticProcessor  _HapticProcessor
	)
	{
		m_ProfileProcessor = _ProfileProcessor;
		m_MenuProcessor    = _MenuProcessor;
		m_HapticProcessor  = _HapticProcessor;
	}

	public async void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
		m_Thumbnail.Setup(m_ProductID);
		
		if (string.IsNullOrEmpty(m_ProductID))
			await HideAsync();
		else if (m_ProfileProcessor.HasProduct(m_ProductID))
			await HideAsync();
		else
			await ShowAsync();
	}

	async void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
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