using UnityEngine;

public class FrameElementEntity : LayoutEntity
{
	public override string  ID   => m_FrameID;
	public override Vector2 Size => m_Pool.Size;

	readonly string              m_FrameID;
	readonly UIFrameElement.Pool m_Pool;

	UIFrameElement m_Item;

	public FrameElementEntity(string _FrameID, UIFrameElement.Pool _Pool)
	{
		m_FrameID = _FrameID;
		m_Pool    = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_FrameID);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}