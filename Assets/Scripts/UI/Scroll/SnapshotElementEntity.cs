using System;
using UnityEngine;

public class SnapshotElementEntity : LayoutEntity
{
	public override string  ID   => m_Snapshot.ID;
	public override Vector2 Size => m_Pool.Size;

	readonly string                 m_Path;
	readonly string                 m_Descriptors;
	readonly Snapshot               m_Snapshot;
	readonly Action<Snapshot>       m_Remove;
	readonly UISnapshotElement.Pool m_Pool;

	UISnapshotElement m_Item;

	public SnapshotElementEntity(
		string                 _Path,
		string                 _Descriptors,
		Snapshot               _Snapshot,
		Action<Snapshot>       _Remove,
		UISnapshotElement.Pool _Pool
	)
	{
		m_Path        = _Path;
		m_Descriptors = _Descriptors;
		m_Snapshot    = _Snapshot;
		m_Remove      = _Remove;
		m_Pool        = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_Path, m_Descriptors, m_Snapshot, m_Remove);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}