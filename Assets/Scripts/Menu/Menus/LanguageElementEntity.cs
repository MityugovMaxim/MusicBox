using UnityEngine;

public class LanguageElementEntity : LayoutEntity
{
	public override string  ID   => m_Localization.Language;
	public override Vector2 Size => m_Pool.Size;

	readonly LocalizationData       m_Localization;
	readonly UILanguageElement.Pool m_Pool;

	UILanguageElement m_Item;

	public LanguageElementEntity(LocalizationData _Localization, UILanguageElement.Pool _Pool)
	{
		m_Localization = _Localization;
		m_Pool         = _Pool;
	}
	
	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_Localization);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}