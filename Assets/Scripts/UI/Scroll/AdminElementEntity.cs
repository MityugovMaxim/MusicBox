using System;
using UnityEngine;

public class AdminElementEntity : LayoutEntity
{
	public override string  ID   => m_Title;
	public override Vector2 Size => m_Pool.Size;

	readonly string              m_Title;
	readonly UIAdminElement.Pool m_Pool;
	readonly Action              m_Action;

	UIAdminElement m_Item;

	public AdminElementEntity(
		string              _Title,
		Action              _Action,
		UIAdminElement.Pool _Pool
	)
	{
		m_Title  = _Title;
		m_Action = _Action;
		m_Pool   = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_Title, m_Action);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}
