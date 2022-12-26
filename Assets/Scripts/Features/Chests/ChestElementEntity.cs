using UnityEngine;

public class ChestElementEntity : LayoutEntity
{
	public override string  ID   => m_ChestID;
	public override Vector2 Size => m_Pool.Size;

	readonly string              m_ChestID;
	readonly UIChestElement.Pool m_Pool;

	UIChestElement m_Item;

	public ChestElementEntity(string _ChestID, UIChestElement.Pool _Pool)
	{
		m_ChestID = _ChestID;
		m_Pool    = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_ChestID);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}