using UnityEngine;

public class ProductItemEntity : LayoutEntity
{
	public override string  ID   { get; }
	public override Vector2 Size { get; }

	readonly string             m_ProductID;
	readonly UIProductItem.Pool m_Pool;

	UIProductItem m_Item;

	public ProductItemEntity(string _ProductID, UIProductItem.Pool _Pool)
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