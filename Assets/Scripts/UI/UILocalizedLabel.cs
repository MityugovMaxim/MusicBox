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
		
		if (m_Data != null && m_Data.Length > 0)
			Label.text = m_LanguageProcessor.Format(m_LocalizationKey, m_Data);
		else
			Label.text = m_LanguageProcessor.Get(m_LocalizationKey);
	}
}
