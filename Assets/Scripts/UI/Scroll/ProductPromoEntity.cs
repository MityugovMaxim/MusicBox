using UnityEngine;

public class ProductPromoEntity : LayoutEntity
{
	public override string  ID   { get; }
	public override Vector2 Size { get; }

	readonly string              m_ProductID;
	readonly UIProductPromo.Pool m_Pool;

	UIProductPromo m_Item;

	public ProductPromoEntity(string _ProductID, UIProductPromo.Pool _Pool)
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