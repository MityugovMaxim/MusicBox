using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class UIBooleanField : UIField
{
	public bool Value
	{
		get => m_Value;
		set
		{
			if (m_Value == value)
				return;
			
			m_Value = value;
			
			SetValue(m_Value);
			
			OnValueChanged();
		}
	}

	[SerializeField] Button m_True;
	[SerializeField] Button m_False;

	bool         m_Value;
	object       m_Object;
	PropertyInfo m_PropertyInfo;

	Func<bool>   m_GetValue;
	Action<bool> m_SetValue;

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
		
		m_GetValue = Delegate.CreateDelegate(typeof(Func<bool>), m_Object, m_PropertyInfo.GetMethod.Name, false, true) as Func<bool>;
		m_SetValue = Delegate.CreateDelegate(typeof(Action<bool>), m_Object, m_PropertyInfo.SetMethod.Name, false, true) as Action<bool>;
		
		Value = GetValue();
		
		m_True.onClick.RemoveAllListeners();
		m_True.onClick.AddListener(Toggle);
		m_True.interactable = m_PropertyInfo.CanWrite;
		
		m_False.onClick.RemoveAllListeners();
		m_False.onClick.AddListener(Toggle);
		m_False.interactable = m_PropertyInfo.CanWrite;
		
		OnValueChanged();
	}

	public void Toggle()
	{
		Value = !Value;
	}

	void OnValueChanged()
	{
		m_True.gameObject.SetActive(Value);
		m_False.gameObject.SetActive(!Value);
	}

	bool GetValue() => m_GetValue?.Invoke() ?? false;

	void SetValue(bool _Value) => m_SetValue?.Invoke(_Value);
}