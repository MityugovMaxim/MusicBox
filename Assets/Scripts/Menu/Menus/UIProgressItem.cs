using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressItem : UIEntity
{
	public int Level => m_Level;

	[SerializeField] TMP_Text m_Label;
	[SerializeField] Button   m_OpenButton;
	[SerializeField] Button   m_RemoveButton;

	int         m_Level;
	Action<int> m_Open;
	Action<int> m_Remove;

	public void Setup(int _Level, Action<int> _Open, Action<int> _Remove)
	{
		m_Level  = _Level;
		m_Open   = _Open;
		m_Remove = _Remove;
		
		m_Label.text = $"LEVEL: {m_Level}";
		
		m_OpenButton.onClick.RemoveAllListeners();
		m_OpenButton.onClick.AddListener(Open);
		
		m_RemoveButton.onClick.RemoveAllListeners();
		m_RemoveButton.onClick.AddListener(Remove);
	}

	void Open()
	{
		m_Open?.Invoke(m_Level);
	}

	void Remove()
	{
		m_Remove?.Invoke(m_Level);
	}
}