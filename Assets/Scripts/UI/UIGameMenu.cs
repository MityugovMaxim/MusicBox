using Zenject;

public class UIGameMenu : UIMenu
{
	[Inject] UIPauseMenu m_PauseMenu;

	public void Pause()
	{
		if (m_PauseMenu != null)
			m_PauseMenu.Toggle();
	}
}