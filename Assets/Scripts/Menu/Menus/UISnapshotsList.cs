using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public abstract class UISnapshotsList<TSnapshot, TEntity> : UIEntity where TSnapshot : Snapshot where TEntity : LayoutEntity 
{
	const float LIST_SPACING = 15;

	protected IReadOnlyList<TSnapshot> Snapshots => m_Snapshots;

	[SerializeField] UILayout m_Content;
	[SerializeField] Button   m_AddButton;

	List<TSnapshot> m_Snapshots;

	protected override void Awake()
	{
		base.Awake();
		
		if (m_AddButton != null)
			m_AddButton.onClick.AddListener(Add);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		if (m_AddButton != null)
			m_AddButton.onClick.RemoveListener(Add);
	}

	public void Setup(List<TSnapshot> _Snapshots)
	{
		m_Snapshots = _Snapshots;
		
		Refresh();
	}

	public void Clear()
	{
		m_Snapshots = null;
		
		Refresh();
	}

	public void Reorder(int _SourceIndex, int _TargetIndex)
	{
		if (_SourceIndex < 0 || _TargetIndex < 0 || _SourceIndex == _TargetIndex)
			return;
		
		TSnapshot snapshot = m_Snapshots[_SourceIndex];
		
		if (snapshot == null)
			return;
		
		m_Snapshots.RemoveAt(_SourceIndex);
		m_Snapshots.Insert(_TargetIndex, snapshot);
		
		for (int i = 0; i < m_Snapshots.Count; i++)
			m_Snapshots[i].Order = i;
		
		Refresh();
	}

	void Refresh()
	{
		m_Content.Clear();
		
		if (m_Snapshots == null || m_Snapshots.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (TSnapshot snapshot in m_Snapshots)
			m_Content.Add(CreateEntity(snapshot, Remove));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Reposition();
	}

	void Add()
	{
		TSnapshot snapshot = CreateSnapshot();
		
		if (snapshot == null)
			return;
		
		m_Snapshots.Add(snapshot);
		
		Refresh();
	}

	void Remove(TSnapshot _Snapshot)
	{
		if (m_Snapshots == null)
			return;
		
		m_Snapshots.Remove(_Snapshot);
		
		Refresh();
	}

	protected abstract TSnapshot CreateSnapshot();

	protected abstract TEntity CreateEntity(TSnapshot _Snapshot, Action<TSnapshot> _Remove);
}

public class UISnapshotsList : UISnapshotsList<Snapshot, SnapshotElementEntity>
{
	[Inject] UISnapshotElement.Pool m_Pool;

	string m_Path;
	string m_Descriptors;

	public void Setup(
		string         _Path,
		string         _Descriptors,
		List<Snapshot> _Snapshots
	)
	{
		m_Path        = _Path;
		m_Descriptors = _Descriptors;
		
		base.Setup(_Snapshots);
	}

	protected override Snapshot CreateSnapshot()
	{
		if (Snapshots == null)
			return null;
		
		Type type = Snapshots.GetType().GetElementType();
		
		if (type == null)
			return null;
		
		return Activator.CreateInstance(type) as Snapshot;
	}

	protected override SnapshotElementEntity CreateEntity(Snapshot _Snapshot, Action<Snapshot> _Remove)
	{
		return new SnapshotElementEntity(m_Path, m_Descriptors, _Snapshot, _Remove, m_Pool);
	}
}