using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UIStringListField : UIField
{
	public IList List
	{
		get => m_List;
	}

	[SerializeField] UIStringListEntry m_Entry;
	[SerializeField] RectTransform     m_Container;

	IList        m_List;
	object       m_Object;
	PropertyInfo m_PropertyInfo;

	public override void Initialize(object _Object, PropertyInfo _PropertyInfo)
	{
		base.Initialize(_Object, _PropertyInfo);
		
		m_Object       = _Object;
		m_PropertyInfo = _PropertyInfo;
		
		m_List = m_PropertyInfo.GetValue(m_Object) as IList;
		
		if (m_List == null)
		{
			m_List = new List<string>();
			m_PropertyInfo.SetValue(m_Object, m_List);
		}
		
		Clear();
		
		Fill();
	}

	public void Add()
	{
		m_List.Add(string.Empty);
		
		Clear();
		
		Fill();
	}

	void RemoveAt(int _Index)
	{
		m_List.RemoveAt(_Index);
		
		Clear();
		
		Fill();
	}

	void Clear()
	{
		int count = m_Container.childCount;
		for (int i = 0; i < count; i++)
			DestroyImmediate(m_Container.GetChild(0).gameObject);
	}

	void Fill()
	{
		for (int i = 0; i < m_List.Count; i++)
		{
			UIStringListEntry entry = Instantiate(m_Entry, m_Container, false);
			
			entry.Initialize(m_List, i, RemoveAt);
		}
	}
}