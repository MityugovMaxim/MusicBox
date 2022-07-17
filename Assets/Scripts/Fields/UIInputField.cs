using TMPro;
using UnityEngine;

public abstract class UIInputField<T> : UIField<T>
{
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
		Value = ParseValue(_Text);
		
		Refresh();
	}

	protected abstract string ParseString(T _Value);

	protected abstract T ParseValue(string _Text);
}