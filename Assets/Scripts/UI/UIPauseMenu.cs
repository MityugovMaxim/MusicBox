using UnityEngine;

public class UIPauseMenu : UIMenu
{
	[SerializeField] UIMainMenu m_MainMenu;

	Level m_Level;

	public void Initialize(Level _Level)
	{
		m_Level = _Level;
	}

	public void Resume()
	{
		Hide();
	}

	public void Restart()
	{
		if (m_Level != null)
			m_Level.Restart();
		
		Hide();
	}

	public void Leave()
	{
		if (m_Level != null)
			Destroy(m_Level.gameObject);
		
		if (m_MainMenu != null)
			m_MainMenu.Show();
	}

	protected override void OnShowStarted()
	{
		if (m_Level != null)
			m_Level.Pause();
	}

	protected override void OnHideStarted()
	{
		if (m_Level != null)
			m_Level.Play();
	}
}
