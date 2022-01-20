using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(TMP_Text))]
public class UILocalizedLabel : UIEntity
{
	TMP_Text Label
	{
		get
		{
			if (m_Label == null)
				m_Label = GetComponent<TMP_Text>();
			return m_Label;
		}
	}

	[SerializeField] string   m_LocalizationKey;
	[SerializeField] bool     m_Trim;
	[SerializeField] string[] m_Data;

	TMP_Text          m_Label;
	LanguageProcessor m_LanguageProcessor;

	[Inject]
	public void Construct(LanguageProcessor _LanguageProcessor)
	{
		m_LanguageProcessor = _LanguageProcessor;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		string text = GetText();
		
		if (m_Trim)
			text = text.Trim();
		
		Label.text = text;
	}

	string GetText()
	{
		if (m_Data == null || m_Data.Length == 0)
			return m_LanguageProcessor.Get(m_LocalizationKey);
		
		switch (m_Data.Length)
		{
			case 1:
				return m_LanguageProcessor.Format(m_LocalizationKey, m_Data[0]);
			case 2:
				return m_LanguageProcessor.Format(m_LocalizationKey, m_Data[0], m_Data[1]);
			case 3:
				return m_LanguageProcessor.Format(m_LocalizationKey, m_Data[0], m_Data[1], m_Data[2]);
			default:
				return m_LanguageProcessor.Format(m_LocalizationKey, m_Data.OfType<object>().ToArray());
		}
	}
}
