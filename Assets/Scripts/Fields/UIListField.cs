using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIListField<T> : UIField<List<T>>, IListField
{
	protected override bool Changed
	{
		get
		{
			if (Value == null || Origin == null)
				return false;
			
			return !Value.SequenceEqual(Origin);
		}
	}

	[SerializeField] TMP_Text m_Label;
	[SerializeField] UIGroup  m_Changed;
	[SerializeField] Button   m_AddButton;

	[Inject] UIListEntry.Factory m_Factory;

	readonly List<UIListEntry> m_Items = new List<UIListEntry>();

	protected override void Awake()
	{
		base.Awake();
		
		m_AddButton.onClick.AddListener(Add);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_AddButton.onClick.RemoveListener(Add);
	}

	protected override void Refresh()
	{
		m_Label.text = Name;
		
		Clear();
		
		Fill();
		
		if (m_Changed == null)
			return;
		
		if (Changed)
			m_Changed.Show();
		else
			m_Changed.Hide();
	}

	void Add()
	{
		Value.Add(default);
		
		Refresh();
	}

	void Clear()
	{
		foreach (UIListEntry item in m_Items)
			Destroy(item.gameObject);
		m_Items.Clear();
	}

	void Fill()
	{
		List<T> value = Value;
		
		if (value == null || value.Count == 0)
			return;
		
		for (int i = 0; i < value.Count; i++)
		{
			UIListEntry item = m_Factory.Create(this, i, RectTransform);
			
			if (item == null)
				continue;
			
			m_Items.Add(item);
		}
	}

	protected override List<T> Cache() => new List<T>(Value);

	object IListField.this[int _Index]
	{
		get => Value[_Index];
		set => Value[_Index] = (T)value;
	}

	Type IListField.EntryType => typeof(T);

	void IListField.Remove(int _Index)
	{
		Value.RemoveAt(_Index);
		
		Refresh();
	}

	void IListField.Modify(int _Index)
	{
		Refresh();
	}
}