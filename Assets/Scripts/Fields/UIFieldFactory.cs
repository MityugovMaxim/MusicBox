using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ModestTree;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class UIFieldFactory : IFactory<object, PropertyInfo, RectTransform, UIField>
{
	static readonly Dictionary<Type, string> m_Types = new Dictionary<Type, string>()
	{
		{ typeof(int), "integer" },
		{ typeof(long), "long" },
		{ typeof(float), "float" },
		{ typeof(double), "double" },
		{ typeof(string), "string" },
		{ typeof(bool), "boolean" },
		{ typeof(Enum), "enum" },
		{ typeof(List<string>), "string_list" },
	};

	static readonly Dictionary<Type, string> m_Attributes = new Dictionary<Type, string>()
	{
		{ typeof(HideInInspector), "hidden" },
		{ typeof(RangeAttribute), "slider" },
	};

	[Inject] DiContainer m_Container;

	public UIField Create(object _Target, PropertyInfo _PropertyInfo, RectTransform _Container)
	{
		Type type = _PropertyInfo.PropertyType;
		
		if (type.IsEnum)
			type = typeof(Enum);
		
		if (!m_Types.TryGetValue(type, out string name) || string.IsNullOrEmpty(name))
			return null;
		
		string path = $"Fields/{name}_field";
		
		UIField prefab = Resources.Load<UIField>(path);
		
		if (prefab == null)
			return null;
		
		UIField field = m_Container.InstantiatePrefabForComponent<UIField>(prefab);
		
		field.RectTransform.SetParent(_Container, false);
		
		field.Setup(_Target, _PropertyInfo);
		
		return field;
	}

	UIField Instantiate(string _Name, PropertyInfo _PropertyInfo)
	{
		if (_PropertyInfo == null)
			return null;
		
		HideInInspector hideAttribute = _PropertyInfo.GetCustomAttribute<HideInInspector>();
		
		if (hideAttribute != null)
			return null;
		
		RangeAttribute rangeAttribute = _PropertyInfo.GetCustomAttribute<RangeAttribute>();
		
		if (rangeAttribute != null)
		{
			UIField sliderField = Resources.Load<UIField>($"Fields/{_Name}_slider");
			
			if (sliderField != null)
			{
				sliderField.SetAttribute(rangeAttribute);
				
				return sliderField;
			}
		}
		
		return Resources.Load<UIField>($"Fields/{_Name}_field");
	}
}