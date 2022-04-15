using TMPro;
using UnityEngine;
using Zenject;

public class UIProductDiscount : UIEntity
{
	[SerializeField] TMP_Text m_Label;

	[Inject] ProductsProcessor m_ProductsProcessor;

	string m_ProductID;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		float discount = m_ProductsProcessor.GetDiscount(m_ProductID);
		
		gameObject.SetActive(!Mathf.Approximately(discount, 0));
		
		string sign = discount > 0 ? "+" : string.Empty;
		
		m_Label.text = $"{sign}{discount}%";
	}
}