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
		m_Price.text = m_StoreProcessor.GetPrice(_ProductID);
	}
}