using UnityEngine;

public class SongElementEntity : LayoutEntity
{
	public override string  ID   => m_SongID;
	public override Vector2 Size => m_Pool.Size;

	readonly string             m_SongID;
	readonly UISongElement.Pool m_Pool;

	UISongElement m_Item;

	public SongElementEntity(string _SongID, UISongElement.Pool _Pool)
	{
		m_SongID = _SongID;
		m_Pool   = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_SongID);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}