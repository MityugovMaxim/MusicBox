using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class UIListEntryFactory : IFactory<IListField, int, RectTransform, UIListEntry>
{
	static readonly Dictionary<Type, string> m_Types = new Dictionary<Type, string>()
	{
		{ typeof(int), "integer" },
		{ typeof(string), "string" },
	};

	[Inject] DiContainer m_Container;

	public UIListEntry Create(IListField _Field, int _Index, RectTransform _Container)
	{
		if (_Field == null)
			return null;
		
		Type type = _Field.EntryType;
		
		if (!m_Types.TryGetValue(type, out string name) || string.IsNullOrEmpty(name))
			return null;
		
		string path = $"Fields/{name}_list_entry";
		
		UIListEntry prefab = Resources.Load<UIListEntry>(path);
		
		if (prefab == null)
			return null;
		
		UIListEntry listEntry = m_Container.InstantiatePrefabForComponent<UIListEntry>(prefab);
		
		listEntry.RectTransform.SetParent(_Container, false);
		
		listEntry.Setup(_Field, _Index);
		
		return listEntry;
	}
}