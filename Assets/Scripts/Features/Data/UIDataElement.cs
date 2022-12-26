using UnityEngine;
using Zenject;

public class UIDataElement : UIEntity
{
	[SerializeField] float m_Spacing;

	[Inject] UIDataNodeFactory m_Factory;

	UIDataEntity m_Item;

	public void Setup(object _Data)
	{
		DataObject data = new DataObject(_Data);
		
		if (m_Item != null)
			m_Factory.Remove(m_Item);
		
		m_Item = m_Factory.Create(data, RectTransform);
		
		Reposition();
	}

	public void Reposition()
	{
		float position = 0;
		
		m_Item.Reposition(ref position, m_Spacing);
		
		Height = position - m_Spacing;
	}
}
