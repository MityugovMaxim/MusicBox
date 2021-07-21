using UnityEngine;

public class UIGameMenu : UIMenu
{
	[SerializeField] UIPauseMenu   m_PauseMenu;
	[SerializeField] UITrackInfo   m_TrackInfo;
	[SerializeField] UITimer       m_Timer;
	[SerializeField] UIProgressBar m_ProgressBar;

	Level m_Level;

	public void Initialize(Level _Level, string _Title, string _Artist)
	{
		m_Level = _Level;
		
		if (m_Level == null)
		{
			Debug.LogError("[UIGameMenu] Initialize failed. Level is null.", gameObject);
			return;
		}
		
		if (m_TrackInfo != null)
			m_TrackInfo.Initialize(_Title, _Artist);
		
		if (m_Timer != null)
			m_Level.OnSample += m_Timer.Process;
		
		if (m_ProgressBar != null)
			m_Level.OnSample += m_ProgressBar.Process;
	}

	public void Pause()
	{
		if (m_PauseMenu != null)
			m_PauseMenu.Toggle();
	}
}