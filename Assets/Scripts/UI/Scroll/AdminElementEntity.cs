using System;
using UnityEngine;

public class AdminElementEntity : LayoutEntity
{
	public override string  ID   => m_Path;
	public override Vector2 Size => m_Pool.Size;

	readonly string              m_Title;
	readonly string              m_Path;
	readonly Type                m_Type;
	readonly UIAdminElement.Pool m_Pool;

	UIAdminElement m_Item;

	public AdminElementEntity(string _Title, string _Path, Type _Type, UIAdminElement.Pool _Pool)
	{
		m_Title = _Title;
		m_Path  = _Path;
		m_Type  = _Type;
		m_Pool  = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_Title, m_Path, m_Type);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}