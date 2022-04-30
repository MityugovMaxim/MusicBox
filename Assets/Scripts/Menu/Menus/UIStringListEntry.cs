using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UIStringListEntry : UIEntity
{
	public string Value
	{
		get => m_Field.text;
		private set => m_Field.text = value;
	}

	[SerializeField] TMP_Text       m_Label;
	[SerializeField] TMP_InputField m_Field;

	IList       m_List;
	int         m_Index;
	Action<int> m_Remove;

	public void Initialize(IList _List, int _Index, Action<int> _Remove)
	{
		m_List   = _List;
		m_Index  = _Index;
		m_Remove = _Remove;
		
		m_Label.text = m_Index.ToString();
		
		m_Field.onValueChanged.RemoveAllListeners();
		m_Field.onValueChanged.AddListener(OnValueChanged);
		
		Value = (string)m_List[m_Index];
	}

	public void Remove()
	{
		m_Remove?.Invoke(m_Index);
	}

	void OnValueChanged(string _Text)
	{
		Value = _Text;
		
		m_List[m_Index] = _Text;
	}
}