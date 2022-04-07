using System;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UILanguageItem : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UILanguageItem> { }

	[SerializeField] UILanguageImage m_Image;
	[SerializeField] TMP_Text        m_Label;

	[Inject] LanguageProcessor m_LanguageProcessor;

	string         m_Language;
	Action<string> m_Select;

	public void Setup(string _Language, Action<string> _Select)
	{
		m_Language = _Language;
		m_Select   = _Select;
		
		m_Image.Setup(_Language);
		
		m_Label.text = m_LanguageProcessor.GetName(m_Language);
	}

	public void Select()
	{
		m_Select?.Invoke(m_Language);
	}
}