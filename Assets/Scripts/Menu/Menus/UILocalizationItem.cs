using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILocalizationItem : UIEntity
{
	public string Key   => m_Key;
	public string Value => m_Value;

	[SerializeField] TMP_Text m_Label;
	[SerializeField] Button   m_OpenButton;
	[SerializeField] Button   m_RemoveButton;

	string         m_Key;
	string         m_Value;
	Action<string> m_Open;
	Action<string> m_Remove;

	public void Setup(
		string         _Key,
		string         _Value,
		Action<string> _Open,
		Action<string> _Remove
	)
	{
		m_Key    = _Key;
		m_Value  = _Value;
		m_Open   = _Open;
		m_Remove = _Remove;
		
		m_Label.text = !string.IsNullOrEmpty(Key) ? Key : "[EMPTY]";
		
		m_OpenButton.onClick.RemoveAllListeners();
		m_OpenButton.onClick.AddListener(Open);
		
		m_RemoveButton.onClick.RemoveAllListeners();
		m_RemoveButton.onClick.AddListener(Remove);
	}

	void Open()
	{
		m_Open?.Invoke(m_Key);
	}

	void Remove()
	{
		m_Remove?.Invoke(m_Key);
	}
}