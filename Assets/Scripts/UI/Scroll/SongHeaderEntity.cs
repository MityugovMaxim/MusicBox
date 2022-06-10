using UnityEngine;
using ColorMode = UIStroke.ColorMode;

public class SongHeaderEntity : LayoutEntity
{
	public override string  ID   => m_Title;
	public override Vector2 Size => m_Pool.Size;

	readonly string            m_Title;
	readonly ColorMode         m_Mode;
	readonly UISongHeader.Pool m_Pool;

	UISongHeader m_Item;

	public SongHeaderEntity(string _Title, ColorMode _Mode, UISongHeader.Pool _Pool)
	{
		m_Title = _Title;
		m_Mode  = _Mode;
		m_Pool  = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Mode = m_Mode;
		
		m_Item.Setup(m_Title);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}