using System;
using UnityEngine;

public class ColorsElementEntity : LayoutEntity
{
	public override string  ID   => m_Snapshot.ID;
	public override Vector2 Size => m_Pool.Size;

	readonly ColorsSnapshot         m_Snapshot;
	readonly Action<ColorsSnapshot> m_Select;
	readonly Action<ColorsSnapshot> m_Remove;
	readonly UIColorsElement.Pool   m_Pool;

	UIColorsElement m_Item;

	public ColorsElementEntity(
		ColorsSnapshot         _Snapshot,
		Action<ColorsSnapshot> _Select,
		Action<ColorsSnapshot> _Remove,
		UIColorsElement.Pool   _Pool
	)
	{
		m_Snapshot = _Snapshot;
		m_Select   = _Select;
		m_Remove   = _Remove;
		m_Pool     = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_Snapshot, m_Select, m_Remove);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}