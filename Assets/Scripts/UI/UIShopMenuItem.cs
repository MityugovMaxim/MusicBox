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

	UIProductMenu m_ProductMenu;

	[Inject]
	public void Construct(UIProductMenu _ProductMenu)
	{
		m_ProductMenu = _ProductMenu;
	}

	public void Setup(string _ProductID)
	{
		ProductID = _ProductID;
		
		m_Thumbnail.Setup(_ProductID);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		m_ProductMenu.Setup(ProductID);
		m_ProductMenu.Show();
	}
}