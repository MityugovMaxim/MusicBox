using UnityEngine;

public class LatencyElementEntity : LayoutEntity
{
	public override string  ID   => "latency_element";
	public override Vector2 Size => new Vector2(600, 60);

	readonly UILatencyElement.Factory m_Factory;

	UILatencyElement m_Item;

	public LatencyElementEntity(UILatencyElement.Factory _Factory)
	{
		m_Factory = _Factory;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Factory.Create("latency_element");
		
		m_Item.gameObject.SetActive(true);
		
		m_Item.RectTransform.SetParent(_Container, false);
		
		m_Item.SetRect(Rect);
	}

	public override void Remove()
	{
		if (m_Item == null)
			return;
		
		m_Item.gameObject.SetActive(true);
	}
}