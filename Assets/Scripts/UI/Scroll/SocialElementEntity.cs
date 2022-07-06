using UnityEngine;

public class SocialElementEntity : LayoutEntity
{
	public override string  ID   => "social_element";
	public override Vector2 Size => m_Pool.Size;

	readonly UISocialElement.Pool m_Pool;

	UISocialElement m_Item;

	public SocialElementEntity(UISocialElement.Pool _Pool)
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