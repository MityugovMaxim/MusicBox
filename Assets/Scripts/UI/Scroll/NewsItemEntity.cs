using UnityEngine;

public class NewsItemEntity : LayoutEntity
{
	public override string  ID   => m_NewsID;
	public override Vector2 Size => m_Pool.Size;

	readonly string          m_NewsID;
	readonly UINewsItem.Pool m_Pool;

	UINewsItem m_Item;

	public NewsItemEntity(string _NewsID, UINewsItem.Pool _Pool)
	{
		m_NewsID = _NewsID;
		m_Pool   = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_NewsID);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}
