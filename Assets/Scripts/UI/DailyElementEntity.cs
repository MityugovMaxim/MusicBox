using UnityEngine;
using Zenject;

public class DailyElementEntity : LayoutEntity
{
	public override string  ID   => "daily";
	public override Vector2 Size => m_Pool.Size;

	[Inject] UIDailyElement.Pool m_Pool;

	UIDailyElement m_Item;

	public DailyElementEntity(UIDailyElement.Pool _Pool)
	{
		m_Pool = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup();
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}