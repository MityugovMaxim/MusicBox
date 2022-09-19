using System;
using System.Text;
using TMPro;
using UnityEngine;

public abstract class UIInputField<T> : UIField<T>
{
	public event Action<T> OnSubmit;

	[SerializeField] TMP_Text       m_Label;
	[SerializeField] TMP_InputField m_Value;
	[SerializeField] UIGroup        m_Changed;

	protected override void Awake()
	{
		base.Awake();
		
		m_Value.onEndEdit.AddListener(Submit);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Value.onEndEdit.RemoveListener(Submit);
	}

	#if UNITY_EDITOR
	protected override void Reset()
	{
		base.Reset();
		
		m_Label = Transform.Find("label").GetComponent<TMP_Text>();
		m_Value = Transform.Find("value").GetComponent<TMP_InputField>();
	}
	#endif

	public void Copy()
	{
		GUIUtility.systemCopyBuffer = ParseString(Value);
	}

	public void Paste()
	{
		Value = ParseValue(GUIUtility.systemCopyBuffer);
		
		Refresh();
	}

	protected override void Refresh()
	{
		m_Label.text = Name;
		m_Value.SetTextWithoutNotify(ParseString(Value));
		
		if (m_Changed == null)
			return;
		
		if (Changed)
			m_Changed.Show();
		else
			m_Changed.Hide();
	}

	void Submit(string _Text)
	{
		string text = Filter(_Text);
		
		Value = ParseValue(text);
		
		Refresh();
		
		OnSubmit?.Invoke(Value);
	}

	string Filter(string _Text)
	{
		if (_Text == null)
			return string.Empty;
		
		if (m_Value == null)
			return _Text;
		
		TMP_FontAsset font = m_Value.fontAsset;
		
		if (font == null)
			return _Text;
		
		StringBuilder filter = new StringBuilder();
		
		foreach (char symbol in _Text)
		{
			if (font.HasCharacter(symbol))
				filter.Append(symbol);
		}
		
		return filter.ToString().Trim();
	}

	protected abstract string ParseString(T _Value);

	protected abstract T ParseValue(string _Text);
}