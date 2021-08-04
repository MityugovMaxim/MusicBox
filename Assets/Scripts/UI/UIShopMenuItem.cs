using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

public class UIShopMenuItem : UIEntity, IPointerClickHandler
{
	[Preserve]
	public class Factory : PlaceholderFactory<UIShopMenuItem, UIShopMenuItem> { }

	[SerializeField] Image    m_Thumbnail;
	[SerializeField] TMP_Text m_Price;

	string            m_ProductID;
	PurchaseProcessor m_PurchaseProcessor;

	[Inject]
	public void Construct(PurchaseProcessor _PurchaseProcessor)
	{
		m_PurchaseProcessor = _PurchaseProcessor;
	}

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		Debug.LogError("---> " + m_PurchaseProcessor.GetTitle(m_ProductID));
		
		if (m_Thumbnail != null)
			m_Thumbnail.sprite = m_PurchaseProcessor.GetPreviewThumbnail(_ProductID);
		
		if (m_Price != null)
			m_Price.text = m_PurchaseProcessor.GetPrice(_ProductID);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		m_PurchaseProcessor.Purchase(m_ProductID);
	}
}