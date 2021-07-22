using UnityEngine;
using Zenject;

public class UIPauseMenu : UIMenu
{
	[Inject] UIMainMenu    m_MainMenu;
	[Inject] LevelProvider m_LevelProvider;

	public void Restart()
	{
		if (m_LevelProvider == null)
		{
			Debug.LogError("[UIPauseMenu] Restart level failed. Level provider is null.", gameObject);
			return;
		}
		
		m_LevelProvider.Stop();
		
		Hide();
	}

	public void Leave()
	{
		if (m_LevelProvider == null)
		{
			Debug.LogError("[UIPauseMenu] Leave level failed. Level provider is null.", gameObject);
			return;
		}
		
		m_LevelProvider.Stop();
		m_LevelProvider.Remove();
		
		if (m_MainMenu != null)
			m_MainMenu.Show();
	}

	protected override void OnShowStarted()
	{
		if (m_LevelProvider != null)
			m_LevelProvider.Pause();
	}

	protected override void OnHideStarted()
	{
		if (m_LevelProvider != null)
			m_LevelProvider.Play();
	}
}
