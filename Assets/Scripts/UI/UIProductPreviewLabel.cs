using TMPro;
using UnityEngine;
using Zenject;

public class UIProductPreviewLabel : UIEntity
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Description;

	PurchaseProcessor m_PurchaseProcessor;

	[Inject]
	public void Construct(PurchaseProcessor _PurchaseProcessor)
	{
		m_PurchaseProcessor = _PurchaseProcessor;
	}

	public void Setup(string _ProductID)
	{
		m_Title.text       = m_PurchaseProcessor.GetTitle(_ProductID);
		m_Description.text = m_PurchaseProcessor.GetDescription(_ProductID);
	}
}