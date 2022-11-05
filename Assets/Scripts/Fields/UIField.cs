using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public abstract class UIField : UIEntity
{
	[Preserve]
	public class Factory : PlaceholderFactory<object, PropertyInfo, RectTransform, UIField> { }

	public event Action OnValueChanged;

	protected string Name => m_Name;

	object       m_Target;
	PropertyInfo m_PropertyInfo;
	string       m_Name;

	public virtual void Setup(object _Target, PropertyInfo _PropertyInfo)
	{
		m_Target       = _Target;
		m_PropertyInfo = _PropertyInfo;
		
		m_Name = m_PropertyInfo?.Name?.ToDisplayName() ?? "[Unknown]";
		
		Refresh();
	}

	public virtual void SetAttribute<TAttribute>(TAttribute _Attribute) where TAttribute : Attribute { }

	public void Setup(object _Target, string _Property)
	{
		if (_Target == null)
			return;
		
		PropertyInfo propertyInfo = _Target.GetType().GetProperty(_Property);
		
		Setup(_Target, propertyInfo);
	}

	public abstract void Restore();

	protected T GetValue<T>()
	{
		return (T)m_PropertyInfo.GetValue(m_Target);
	}

	protected void SetValue<T>(T _Value)
	{
		if (m_PropertyInfo == null || m_Target == null)
			return;
		
		if (m_PropertyInfo.CanWrite)
		{
			m_PropertyInfo.SetValue(m_Target, _Value);
			OnValueChanged?.Invoke();
			return;
		}
		
		Type type = m_PropertyInfo.DeclaringType;
		if (type == null)
			return;
		
		FieldInfo fieldInfo = type.GetField(
			$"<{m_PropertyInfo.Name}>k__BackingField",
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly
		);
		
		if (fieldInfo == null)
			return;
		
		fieldInfo.SetValue(m_Target, _Value);
		OnValueChanged?.Invoke();
	}

	protected abstract void Refresh();
}

public abstract class UIField<T> : UIField
{
	protected T Value
	{
		get => GetValue<T>();
		set => SetValue(value);
	}

	protected virtual bool Changed
	{
		get
		{
			if (Value == null ^ Origin == null)
				return true;
			
			if (Value == null && Origin == null)
				return false;
			
			return !Value.Equals(Origin);
		}
	}

	protected T Origin { get; private set; }

	public override void Setup(object _Target, PropertyInfo _PropertyInfo)
	{
		base.Setup(_Target, _PropertyInfo);
		
		Origin = Cache();
		
		Refresh();
	}

	public override void Restore()
	{
		Value = Origin;
		
		Origin = Cache();
		
		Refresh();
	}

	protected virtual T Cache() => GetValue<T>();
}
