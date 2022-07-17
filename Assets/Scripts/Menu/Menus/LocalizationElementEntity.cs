using UnityEngine;

public class LocalizationElementEntity : LayoutEntity
{
	public override string  ID   => m_Key;
	public override Vector2 Size => m_Pool.Size;

	readonly string                     m_Key;
	readonly LocalizationData           m_Localization;
	readonly UILocalizationElement.Pool m_Pool;

	UILocalizationElement m_Item;

	public LocalizationElementEntity(string _Key, LocalizationData _Localization, UILocalizationElement.Pool _Pool)
	{
		m_Key          = _Key;
		m_Localization = _Localization;
		m_Pool         = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_Key, m_Localization);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}