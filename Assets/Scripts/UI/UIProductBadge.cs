using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIProductBadge : UIEntity
{
	const string LOCALIZATION_PREFIX = "LANG:";

	[SerializeField] TMP_Text m_Label;
	[SerializeField] Graphic  m_Background;

	[Inject] ProductsProcessor     m_ProductsProcessor;
	[Inject] LocalizationProcessor m_LocalizationProcessor;

	string m_ProductID;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		string badge = m_ProductsProcessor.GetBadge(m_ProductID);
		
		if (string.IsNullOrEmpty(badge))
		{
			gameObject.SetActive(false);
			return;
		}
		
		gameObject.SetActive(true);
		
		if (badge.StartsWith(LOCALIZATION_PREFIX))
			badge = m_LocalizationProcessor.Get(badge[LOCALIZATION_PREFIX.Length..]);
		
		Color color = m_ProductsProcessor.GetColor(m_ProductID);
		
		m_Label.text       = badge;
		m_Background.color = color;
	}
}