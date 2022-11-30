using UnityEngine;

public class SeasonLevelElementEntity : LayoutEntity
{
	public override string  ID   => $"{m_SeasonID}_{m_Level}";
	public override Vector2 Size => m_Pool.Size;

	readonly string                    m_SeasonID;
	readonly int                       m_Level;
	readonly UISeasonLevelElement.Pool m_Pool;

	UISeasonLevelElement m_Item;

	public SeasonLevelElementEntity(string _SeasonID, int _Level, UISeasonLevelElement.Pool _Pool)
	{
		m_SeasonID = _SeasonID;
		m_Level    = _Level;
		m_Pool     = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_SeasonID, m_Level);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}
