using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Zenject;

public class UIShopMenuItem : UIEntity, IPointerClickHandler
{
	[Preserve]
	public class Factory : PlaceholderFactory<UIShopMenuItem, UIShopMenuItem> { }

	public string ProductID { get; private set; }

	[SerializeField] UIProductPreviewThumbnail m_Thumbnail;

	MenuProcessor m_MenuProcessor;

	[Inject]
	public void Construct(MenuProcessor _MenuProcessor)
	{
		m_MenuProcessor = _MenuProcessor;
	}

	public void Setup(string _ProductID)
	{
		ProductID = _ProductID;
		
		m_Thumbnail.Setup(_ProductID);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>(MenuType.ProductMenu);
		
		if (productMenu == null)
			return;
		
		productMenu.Setup(ProductID);
		productMenu.Show();
	}
}