using System;
using UnityEngine;
using Zenject;

public class UIResultMenu : UIMenu, IInitializable, IDisposable
{
	SignalBus      m_SignalBus;
	UIMainMenu     m_MainMenu;
	LevelProvider  m_LevelProvider;
	ScoreProcessor m_ScoreProcessor;

	[Inject]
	public void Construct(
		SignalBus      _SignalBus,
		UIMainMenu     _MainMenu,
		LevelProvider  _LevelProvider,
		ScoreProcessor _ScoreProcessor
	)
	{
		m_SignalBus      = _SignalBus;
		m_MainMenu       = _MainMenu;
		m_LevelProvider  = _LevelProvider;
		m_ScoreProcessor = _ScoreProcessor;
	}

	public void Restart()
	{
		// TODO: Add advertisement here
		
		if (m_LevelProvider == null)
		{
			Debug.LogError("[UIResultMenu] Restart level failed. Level provider is null.", gameObject);
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
			Debug.LogError("[UIResultMenu] Leave level failed. Level provider is null.", gameObject);
			return;
		}
		
		m_LevelProvider.Stop();
		m_LevelProvider.Remove();
		
		if (m_MainMenu != null)
			m_MainMenu.Show();
	}

	public void Next()
	{
		// TODO: Add advertisement here
		
		if (m_LevelProvider == null)
		{
			Debug.LogError("[UIResultMenu] Leave level failed. Level provider is null.", gameObject);
			return;
		}
		
		m_LevelProvider.Stop();
		m_LevelProvider.Remove();
		
		if (m_MainMenu != null)
		{
			m_MainMenu.NextPreview();
			m_MainMenu.Show();
		}
	}

	protected override void OnShowFinished()
	{
		// Score animation
		// Save score
		// Show buttons
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelCompleteSignal>(RegisterLevelComplete);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelCompleteSignal>(RegisterLevelComplete);
	}

	void RegisterLevelComplete()
	{
		// Load score
		// Restore score view
		
		Show();
	}
}