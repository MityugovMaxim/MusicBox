using UnityEngine;

public class ProfileElementEntity : LayoutEntity
{
	public override string  ID   => "profile";
	public override Vector2 Size => m_Pool.Size;

	readonly UIProfileElement.Pool m_Pool;

	UIProfileElement m_Item;

	public ProfileElementEntity(UIProfileElement.Pool _Pool)
	{
		m_Pool = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}
