using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIProductPreviewThumbnail : UIEntity
{
	[SerializeField] Image m_Thumbnail;

	PurchaseProcessor m_PurchaseProcessor;

	[Inject]
	public void Construct(PurchaseProcessor _PurchaseProcessor)
	{
		m_PurchaseProcessor = _PurchaseProcessor;
	}

	public void Setup(string _ProductID)
	{
		m_Thumbnail.sprite = m_PurchaseProcessor.GetPreviewThumbnail(_ProductID);
	}
}