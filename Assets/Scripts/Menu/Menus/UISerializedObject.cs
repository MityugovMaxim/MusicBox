using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property)]
public class TextPropertyAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class ClipboardPropertyAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class HidePropertyAttribute : Attribute { }

public class UISerializedObject : UIEntity
{
	[SerializeField] RectTransform     m_Container;
	[SerializeField] UILabelField      m_LabelField;
	[SerializeField] UIIntegerField    m_IntegerField;
	[SerializeField] UILongField       m_LongField;
	[SerializeField] UIFloatField      m_FloatField;
	[SerializeField] UIDoubleField     m_DoubleField;
	[SerializeField] UIStringField     m_StringField;
	[SerializeField] UIStringField     m_StringClipboardField;
	[SerializeField] UIStringField     m_TextField;
	[SerializeField] UIBooleanField    m_BooleanField;
	[SerializeField] UIEnumField       m_EnumField;
	[SerializeField] UIStringListField m_StringListField;

	object m_Object;

	public void Add(string _Name, object _Object)
	{
		m_Object = _Object;
		
		Type type = m_Object.GetType();
		
		UILabelField label = Instantiate(m_LabelField, m_Container, false);
		
		label.Value = _Name;
		
		PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
		foreach (PropertyInfo propertyInfo in propertyInfos)
		{
			if (!propertyInfo.CanWrite)
				continue;
			
			Attribute[] attributes = propertyInfo.GetCustomAttributes().ToArray();
			
			UIField asset = GetField(propertyInfo.PropertyType, attributes);
			
			if (asset == null)
				continue;
			
			UIField field = Instantiate(asset, m_Container, false);
			
			field.Initialize(m_Object, propertyInfo);
		}
	}

	public void Clear()
	{
		int childCount = m_Container.childCount;
		for (int i = 0; i < childCount; i++)
			DestroyImmediate(m_Container.GetChild(0).gameObject);
	}

	UIField GetField(Type _Type, Attribute[] _Attributes)
	{
		if (_Attributes.OfType<HidePropertyAttribute>().Any())
			return null;
		
		if (_Type == typeof(bool))
			return m_BooleanField;
		
		if (_Type == typeof(int))
			return m_IntegerField;
		
		if (_Type == typeof(long))
			return m_LongField;
		
		if (_Type == typeof(float))
			return m_FloatField;
		
		if (_Type == typeof(double))
			return m_DoubleField;
		
		if (_Type == typeof(string))
		{
			if (_Attributes.OfType<TextPropertyAttribute>().Any())
				return m_TextField;
			else if (_Attributes.OfType<ClipboardPropertyAttribute>().Any())
				return m_StringClipboardField;
			else
				return m_StringField;
		}
		
		if (_Type.IsEnum)
			return m_EnumField;
		
		if (_Type == typeof(List<string>))
			return m_StringListField;
		
		return null;
	}
}