using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Zenject;

public class UIStoreItem : UIEntity, IPointerClickHandler
{
	[Preserve]
	public class Pool : MonoMemoryPool<UIStoreItem> { }

	[SerializeField] UIProductThumbnail m_Thumbnail;
	[SerializeField] UIProductDiscount  m_Discount;
	[SerializeField] UIProductPrice     m_Price;

	MenuProcessor m_MenuProcessor;

	string m_ProductID;

	[Inject]
	public void Construct(MenuProcessor _MenuProcessor)
	{
		m_MenuProcessor = _MenuProcessor;
	}

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Thumbnail.Setup(m_ProductID);
		m_Discount.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		
		if (productMenu == null)
			return;
		
		productMenu.Setup(m_ProductID);
		productMenu.Show();
	}
}