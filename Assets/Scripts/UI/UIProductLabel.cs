using TMPro;
using UnityEngine;
using Zenject;

public class UIProductLabel : UIEntity
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Description;

	[Inject] ProductsProcessor m_ProductsProcessor;

	public void Setup(string _ProductID)
	{
		m_Title.text       = m_ProductsProcessor.GetTitle(_ProductID);
		m_Description.text = m_ProductsProcessor.GetDescription(_ProductID);
	}
}