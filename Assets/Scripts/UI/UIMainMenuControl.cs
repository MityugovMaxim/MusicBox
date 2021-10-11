using System.Collections;
using UnityEngine;

public class UIMainMenuControl : UIEntity
{
	[SerializeField] UIMainMenuButton[] m_Buttons;
	[SerializeField] UIMainMenuRing     m_Ring;

	IEnumerator m_MoveRoutine;

	public void Select(MainMenuPageType _PageType, bool _Instant = false)
	{
		foreach (UIMainMenuButton button in m_Buttons)
			button.Toggle(button.PageType == _PageType, _Instant);
		
		foreach (UIMainMenuButton button in m_Buttons)
		{
			if (button.PageType != _PageType)
				continue;
			
			m_Ring.Move(button.RectTransform, _Instant);
			
			break;
		}
	}
}