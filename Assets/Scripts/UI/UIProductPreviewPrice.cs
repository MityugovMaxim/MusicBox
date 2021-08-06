using TMPro;
using UnityEngine;
using Zenject;

public class UIProductPreviewPrice : UIEntity
{
	[SerializeField] TMP_Text m_Price;

	PurchaseProcessor m_PurchaseProcessor;

	[Inject]
	public void Construct(PurchaseProcessor _PurchaseProcessor)
	{
		m_PurchaseProcessor = _PurchaseProcessor;
	}

	public void Setup(string _ProductID)
	{
		m_Price.text = m_PurchaseProcessor.GetPrice(_ProductID);
	}
}