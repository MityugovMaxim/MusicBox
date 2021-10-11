using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

[RequireComponent(typeof(CanvasGroup))]
public class UIProductPromo : UIGroup, IPointerClickHandler
{
	[SerializeField] UIProductLabel     m_Label;
	[SerializeField] UIProductPrice     m_Price;
	[SerializeField] UIProductThumbnail m_Thumbnail;

	StoreProcessor  m_StoreProcessor;
	HapticProcessor m_HapticProcessor;
	MenuProcessor   m_MenuProcessor;

	string m_ProductID;

	[Inject]
	public void Construct(
		StoreProcessor  _StoreProcessor,
		MenuProcessor   _MenuProcessor,
		HapticProcessor _HapticProcessor
	)
	{
		m_StoreProcessor  = _StoreProcessor;
		m_MenuProcessor   = _MenuProcessor;
		m_HapticProcessor = _HapticProcessor;
	}

	public async void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		if (string.IsNullOrEmpty(m_ProductID))
		{
			Hide();
			return;
		}
		
		if (m_StoreProcessor.IsProductPurchased(m_ProductID))
		{
			Hide();
			return;
		}
		
		if (m_StoreProcessor.Loaded)
		{
			m_Label.Setup(m_ProductID);
			m_Price.Setup(m_ProductID);
			m_Thumbnail.Setup(m_ProductID);
			Show();
			return;
		}
		
		try
		{
			await m_StoreProcessor.LoadStore();
			
			m_Label.Setup(m_ProductID);
			m_Price.Setup(m_ProductID);
			m_Thumbnail.Setup(m_ProductID);
			Show();
		}
		catch
		{
			Hide();
		}
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