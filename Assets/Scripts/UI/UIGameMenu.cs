using UnityEngine;
using Zenject;

public class UIGameMenu : UIMenu
{
	[SerializeField] UIControl m_Control;

	[Inject] UIPauseMenu m_PauseMenu;

	public void Pause()
	{
		if (m_PauseMenu == null)
			return;
		
		if (m_PauseMenu.Shown)
		{
			m_Control.Locked = false;
			m_PauseMenu.Resume();
		}
		else
		{
			m_Control.Locked = true;
			m_PauseMenu.Pause();
		}
	}
}