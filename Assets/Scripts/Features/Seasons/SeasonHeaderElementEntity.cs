using UnityEngine;

public class SeasonHeaderElementEntity : LayoutEntity
{
	public override string  ID   => m_SeasonID;
	public override Vector2 Size => m_Pool.Size;

	readonly string                     m_SeasonID;
	readonly UISeasonHeaderElement.Pool m_Pool;

	UISeasonHeaderElement m_Item;

	public SeasonHeaderElementEntity(string _SeasonID, UISeasonHeaderElement.Pool _Pool)
	{
		m_SeasonID = _SeasonID;
		m_Pool     = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.SeasonID = m_SeasonID;
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}