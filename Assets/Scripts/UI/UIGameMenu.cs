using System;
using UnityEngine;
using Zenject;

public class UIGameMenu : UIMenu, IInitializable, IDisposable
{
	[SerializeField] UIControl m_Control;

	SignalBus   m_SignalBus;
	UIPauseMenu m_PauseMenu;

	[Inject]
	public void Construct(
		SignalBus   _SignalBus,
		UIPauseMenu _PauseMenu
	)
	{
		m_SignalBus = _SignalBus;
		m_PauseMenu = _PauseMenu;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelExitSignal>(RegisterLevelExit);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelExitSignal>(RegisterLevelExit);
	}

	public void Pause()
	{
		if (m_PauseMenu == null)
			return;
		
		if (m_PauseMenu.Shown)
		{
			// TODO: Remove this fucking bool
			m_Control.Locked = false;
			m_PauseMenu.Resume();
		}
		else
		{
			// TODO: Remove this fucking bool
			m_Control.Locked = true;
			m_PauseMenu.Pause();
		}
	}

	void RegisterLevelStart()
	{
		Show(true);
	}

	void RegisterLevelExit()
	{
		Hide(true);
	}
}