using UnityEngine;

public class VoucherElementEntity : LayoutEntity
{
	public override string  ID   => m_VoucherID;
	public override Vector2 Size => m_Pool.Size;

	readonly string                m_VoucherID;
	readonly UIVoucherElement.Pool m_Pool;

	UIVoucherElement m_Item;

	public VoucherElementEntity(string _VoucherID, UIVoucherElement.Pool _Pool)
	{
		m_VoucherID = _VoucherID;
		m_Pool      = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(ID);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}