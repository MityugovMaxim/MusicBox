using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAmbientItem : UIEntity
{
	public string AmbientID => m_AmbientID;

	[SerializeField] TMP_Text m_Label;
	[SerializeField] Button   m_OpenButton;
	[SerializeField] Button   m_RemoveButton;

	string         m_AmbientID;
	Action<string> m_Open;
	Action<string> m_Remove;

	public void Setup(string _AmbientID, Action<string> _Open, Action<string> _Remove)
	{
		m_AmbientID = _AmbientID;
		m_Open      = _Open;
		m_Remove    = _Remove;
		
		m_Label.text = !string.IsNullOrEmpty(AmbientID) ? AmbientID : "[EMPTY]";
		
		m_OpenButton.onClick.RemoveAllListeners();
		m_OpenButton.onClick.AddListener(Open);
		
		m_RemoveButton.onClick.RemoveAllListeners();
		m_RemoveButton.onClick.AddListener(Remove);
	}

	void Open()
	{
		m_Open?.Invoke(AmbientID);
	}

	void Remove()
	{
		m_Remove?.Invoke(AmbientID);
	}
}