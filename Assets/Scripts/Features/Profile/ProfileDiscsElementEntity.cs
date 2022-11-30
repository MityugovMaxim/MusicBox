using UnityEngine;

public class ProfileDiscsElementEntity : LayoutEntity
{
	public override string  ID   => "profile_discs";
	public override Vector2 Size => m_Pool.Size;

	readonly UIProfileDiscsElement.Pool m_Pool;

	UIProfileDiscsElement m_Item;

	public ProfileDiscsElementEntity(UIProfileDiscsElement.Pool _Pool)
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