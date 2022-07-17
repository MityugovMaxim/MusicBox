using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEnumField : UIField<Enum>
{
	[SerializeField] TMP_Text m_Label;
	[SerializeField] TMP_Text m_Value;
	[SerializeField] UIGroup  m_Changed;
	[SerializeField] Button   m_NextButton;
	[SerializeField] Button   m_PreviousButton;

	protected override void Awake()
	{
		base.Awake();
		
		m_NextButton.onClick.AddListener(Next);
		m_PreviousButton.onClick.AddListener(Previous);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_NextButton.onClick.RemoveListener(Next);
		m_PreviousButton.onClick.RemoveListener(Previous);
	}

	protected override void Refresh()
	{
		m_Label.text = Name;
		m_Value.text = Value.ToString();
		
		ProcessButtons();
		
		if (m_Changed == null)
			return;
		
		if (Changed)
			m_Changed.Show();
		else
			m_Changed.Hide();
	}

	void Next() => Select(1);

	void Previous() => Select(-1);

	void Select(int _Offset)
	{
		string   value  = Value.ToString();
		Type     type   = Value.GetType();
		string[] values = Enum.GetNames(type);
		int      index  = Mathf.Clamp(Array.FindIndex(values, _Value => _Value == value) + _Offset, 0, values.Length - 1);
		
		Value = (Enum)Enum.Parse(type, values[index]);
		
		Refresh();
	}

	void ProcessButtons()
	{
		string   value  = Value.ToString();
		Type     type   = Value.GetType();
		string[] values = Enum.GetNames(type);
		int      index  = Mathf.Clamp(Array.FindIndex(values, _Value => _Value == value), 0, values.Length - 1);
		
		m_NextButton.interactable     = index < values.Length - 1;
		m_PreviousButton.interactable = index > 0;
	}
}