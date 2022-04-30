using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ModestTree;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEnumField : UIField
{
	public int Value
	{
		get => m_Value;
		private set
		{
			if (m_Value == value)
				return;
			
			m_Value = value;
			
			SetValue(m_Value);
			
			OnValueChanged();
		}
	}

	[SerializeField] TMP_Text m_Field;
	[SerializeField] Button   m_Previous;
	[SerializeField] Button   m_Next;

	int          m_Value;
	Type         m_Type;
	object       m_Object;
	PropertyInfo m_PropertyInfo;

	Func<int>      m_GetValue;
	Action<object> m_SetValue;

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
		
		m_GetValue = Delegate.CreateDelegate(typeof(Func<int>), m_Object, m_PropertyInfo.GetMethod.Name, false, true) as Func<int>;
		
		m_SetValue = _Value => m_PropertyInfo.SetValue(m_Object, Value);
		
		m_Type = m_PropertyInfo.PropertyType;
		
		m_Previous.onClick.RemoveAllListeners();
		m_Previous.onClick.AddListener(Previous);
		m_Previous.interactable = m_PropertyInfo.CanWrite;
		
		m_Next.onClick.RemoveAllListeners();
		m_Next.onClick.AddListener(Next);
		m_Next.interactable = m_PropertyInfo.CanWrite;
		
		Value = GetValue();
		
		OnValueChanged();
	}

	public void Previous()
	{
		SelectValue(-1);
	}

	public void Next()
	{
		SelectValue(1);
	}

	void SelectValue(int _Offset)
	{
		int[] values = Enum.GetValues(m_Type).Cast<int>().ToArray();
		
		int index = values.IndexOf(Value);
		
		if (index < 0)
			index = 0;
		
		index = MathUtility.Repeat(index + _Offset, values.Length);
		
		Value = values[index];
	}

	void OnValueChanged()
	{
		m_Field.text = Enum.GetName(m_Type, Value);
	}

	int GetValue() => m_GetValue?.Invoke() ?? 0;

	void SetValue(object _Value) => m_SetValue?.Invoke(_Value);
}