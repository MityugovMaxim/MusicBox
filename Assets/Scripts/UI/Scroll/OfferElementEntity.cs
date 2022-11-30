using UnityEngine;

public class OfferElementEntity : LayoutEntity
{
	public override string  ID   => m_OfferID;
	public override Vector2 Size => m_Pool.Size;

	readonly string           m_OfferID;
	readonly UIOfferElement.Pool m_Pool;

	UIOfferElement m_Item;

	public OfferElementEntity(string _OfferID, UIOfferElement.Pool _Pool)
	{
		m_OfferID = _OfferID;
		m_Pool    = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_OfferID);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}

	public override void Refresh()
	{
		if (m_Item != null)
			m_Item.Setup(m_OfferID);
	}
}
