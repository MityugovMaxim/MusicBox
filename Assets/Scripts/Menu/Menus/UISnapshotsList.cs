using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UISnapshotsList : UIEntity
{
	const float LIST_SPACING = 15;

	[SerializeField] UILayout m_Content;
	[SerializeField] Button   m_AddButton;

	[Inject] UISnapshotElement.Pool m_Pool;

	string         m_Path;
	List<Snapshot> m_Snapshots;

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

	public void Setup(string _Path, List<Snapshot> _Snapshots)
	{
		m_Path      = _Path;
		m_Snapshots = _Snapshots;
		
		Refresh();
	}

	void Refresh()
	{
		m_Content.Clear();
		
		if (m_Snapshots == null || m_Snapshots.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (Snapshot snapshot in m_Snapshots)
			m_Content.Add(new SnapshotElementEntity(m_Path, snapshot, Remove, m_Pool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Reposition();
	}

	void Add()
	{
		if (m_Snapshots == null)
			return;
		
		Type type = m_Snapshots.GetType().GetElementType();
		
		if (type == null)
			return;
		
		Snapshot snapshot = Activator.CreateInstance(type) as Snapshot;
		
		m_Snapshots.Add(snapshot);
		
		Refresh();
	}

	void Remove(Snapshot _Snapshot)
	{
		if (m_Snapshots == null)
			return;
		
		m_Snapshots.Remove(_Snapshot);
		
		Refresh();
	}

	public void Reorder(int _SourceIndex, int _TargetIndex)
	{
		if (_SourceIndex < 0 || _TargetIndex < 0 || _SourceIndex == _TargetIndex)
			return;
		
		Snapshot snapshot = m_Snapshots[_SourceIndex];
		
		if (snapshot == null)
			return;
		
		m_Snapshots.RemoveAt(_SourceIndex);
		m_Snapshots.Insert(_TargetIndex, snapshot);
		
		for (int i = 0; i < m_Snapshots.Count; i++)
			m_Snapshots[i].Order = i;
		
		Refresh();
	}
}