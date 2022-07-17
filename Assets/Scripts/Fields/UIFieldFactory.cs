using System;
using System.Collections.Generic;
using System.Reflection;
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
}