using UnityEngine;

public class UIMainMenuControl : UIEntity
{
	[SerializeField] UIMainMenuButton[] m_Buttons;
	[SerializeField] UIMainMenuRing     m_Ring;

	MainMenuPageType m_PageType;

	public void Select(MainMenuPageType _PageType, bool _Instant)
	{
		m_PageType = _PageType;
		
		foreach (UIMainMenuButton button in m_Buttons)
			button.Toggle(button.PageType == m_PageType, _Instant);
		
		foreach (UIMainMenuButton button in m_Buttons)
		{
			if (button.PageType != m_PageType)
				continue;
			
			m_Ring.Move(button.RectTransform, _Instant);
			
			break;
		}
	}
}
