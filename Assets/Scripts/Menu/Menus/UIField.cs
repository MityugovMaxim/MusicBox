using System;
using System.Reflection;
using TMPro;
using UnityEngine;

public abstract class UIField : UIEntity
{
	[SerializeField] TMP_Text m_Label;

	public virtual void Initialize(object _Object, PropertyInfo _PropertyInfo)
	{
		m_Label.text = _PropertyInfo != null ? _PropertyInfo.Name.GetDisplayName() : "null";
	}
}

public abstract class UIField<T> : UIField
{
	public T Value
	{
		get => Parse(m_Field.text);
		private set => m_Field.text = value?.ToString();
	}

	protected abstract TMP_InputField.ContentType ContentType { get; }

	protected abstract TouchScreenKeyboardType KeyboardType { get; }

	[SerializeField] TMP_InputField m_Field;

	object       m_Object;
	PropertyInfo m_PropertyInfo;

	Func<T>   m_GetValue;
	Action<T> m_SetValue;

	public override void Initialize(object _Object, PropertyInfo _PropertyInfo)
	{
		base.Initialize(_Object, _PropertyInfo);
		
		m_Object       = _Object;
		m_PropertyInfo = _PropertyInfo;
		
		if (m_Object == null || m_PropertyInfo == null)
		{
			Value = default;
			return;
		}
		
		m_GetValue = m_PropertyInfo.CanRead
			? Delegate.CreateDelegate(typeof(Func<T>), m_Object, m_PropertyInfo.GetMethod.Name, false, false) as Func<T>
			: null;
		
		m_SetValue = m_PropertyInfo.CanWrite
			? Delegate.CreateDelegate(typeof(Action<T>), m_Object, m_PropertyInfo.SetMethod.Name, false, false) as Action<T>
			: null;
		
		m_Field.contentType  = ContentType;
		m_Field.keyboardType = KeyboardType;
		
		m_Field.onSubmit.RemoveAllListeners();
		m_Field.onSubmit.AddListener(OnValueChanged);
		
		m_Field.readOnly = m_SetValue == null;
		
		Value = GetValue();
	}

	public void Copy()
	{
		GUIUtility.systemCopyBuffer = Value?.ToString() ?? string.Empty;
	}

	public void Paste()
	{
		Value = Parse(GUIUtility.systemCopyBuffer);
	}

	void OnValueChanged(string _Text)
	{
		SetValue(Parse(_Text));
	}

	protected abstract T Parse(string _Text);

	T GetValue() => m_GetValue != null ? m_GetValue.Invoke() : default;

	void SetValue(T _Value) => m_SetValue?.Invoke(_Value);
}