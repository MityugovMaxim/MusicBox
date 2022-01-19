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
		
		if (m_Data == null || m_Data.Length == 0)
		{
			Label.text = m_LanguageProcessor.Get(m_LocalizationKey);
			return;
		}
		
		switch (m_Data.Length)
		{
			case 1:
				Label.text = m_LanguageProcessor.Format(m_LocalizationKey, m_Data[0]);
				break;
			case 2:
				Label.text = m_LanguageProcessor.Format(m_LocalizationKey, m_Data[0], m_Data[1]);
				break;
			case 3:
				Label.text = m_LanguageProcessor.Format(m_LocalizationKey, m_Data[0], m_Data[1], m_Data[2]);
				break;
			default:
				Label.text = m_LanguageProcessor.Format(m_LocalizationKey, m_Data.OfType<object>().ToArray());
				break;
		}
	}
}
