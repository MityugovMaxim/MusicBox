using TMPro;
using UnityEngine;
using Zenject;

public class UIProductPrice : UIEntity
{
	[SerializeField] TMP_Text m_Price;

	StoreProcessor m_StoreProcessor;

	[Inject]
	public void Construct(StoreProcessor _StoreProcessor)
	{
		m_StoreProcessor = _StoreProcessor;
	}

	public void Setup(string _ProductID)
	{
		if (m_Price == null || m_Price.font == null)
			return;
		
		string price = m_StoreProcessor.GetPrice(_ProductID);
		
		if (!m_Price.font.HasCharacters(price))
			price = m_StoreProcessor.GetPrice(_ProductID, false);
		
		m_Price.text = price;
	}
}