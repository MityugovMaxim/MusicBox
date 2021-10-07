using TMPro;
using UnityEngine;
using Zenject;

public class UIProductLabel : UIEntity
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Description;

	StoreProcessor m_StoreProcessor;

	[Inject]
	public void Construct(StoreProcessor _StoreProcessor)
	{
		m_StoreProcessor = _StoreProcessor;
	}

	public void Setup(string _ProductID)
	{
		m_Title.text       = m_StoreProcessor.GetTitle(_ProductID);
		m_Description.text = m_StoreProcessor.GetDescription(_ProductID);
	}
}