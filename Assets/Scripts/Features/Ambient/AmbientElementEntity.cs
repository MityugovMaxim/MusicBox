using UnityEngine;

public class AmbientElementEntity : LayoutEntity
{
	public override string  ID   => "ambient";
	public override Vector2 Size => m_Pool.Size;

	readonly UIAmbientElement.Pool m_Pool;

	UIAmbientElement m_Item;

	public AmbientElementEntity(UIAmbientElement.Pool _Pool)
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
