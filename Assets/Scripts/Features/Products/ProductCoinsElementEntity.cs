using UnityEngine;

public class ProductCoinsElementEntity : LayoutEntity
{
	public override string  ID   => m_ProductID;
	public override Vector2 Size => m_Pool.Size;

	readonly string                     m_ProductID;
	readonly UIProductCoinsElement.Pool m_Pool;

	UIProductCoinsElement m_Item;

	public ProductCoinsElementEntity(string _ProductID, UIProductCoinsElement.Pool _Pool)
	{
		m_ProductID = _ProductID;
		m_Pool      = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_ProductID);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}
