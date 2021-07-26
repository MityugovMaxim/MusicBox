using System;
using UnityEngine;
using Zenject;

public class UIPauseMenu : UIMenu
{
	[Inject] UIMainMenu    m_MainMenu;
	[Inject] LevelProvider m_LevelProvider;

	public void Pause()
	{
		if (m_LevelProvider != null)
			m_LevelProvider.Pause();
		
		Show();
	}

	public void Resume()
	{
		if (m_LevelProvider != null)
			m_LevelProvider.Play();
		
		Hide();
	}

	public void Restart()
	{
		if (m_LevelProvider == null)
		{
			Debug.LogError("[UIPauseMenu] Restart level failed. Level provider is null.", gameObject);
			return;
		}
		
		m_LevelProvider.Stop();
		
		CloseAction = m_LevelProvider.Play;
		
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
}
