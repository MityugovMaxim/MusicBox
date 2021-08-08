using System;
using UnityEngine;
using Zenject;

public class UIGameMenu : UIMenu, IInitializable, IDisposable
{
	[SerializeField] UIGamePauseButton m_PauseButton;

	SignalBus m_SignalBus;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Subscribe<LevelExitSignal>(RegisterLevelExit);
		m_SignalBus.Subscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
		m_SignalBus.Subscribe<AudioPlaySignal>(RegisterAudioPlaySignal);
		m_SignalBus.Subscribe<AudioPauseSignal>(RegisterAudioPauseSignal);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Unsubscribe<LevelExitSignal>(RegisterLevelExit);
		m_SignalBus.Unsubscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
		m_SignalBus.Unsubscribe<AudioPlaySignal>(RegisterAudioPlaySignal);
		m_SignalBus.Unsubscribe<AudioPauseSignal>(RegisterAudioPauseSignal);
	}

	void RegisterLevelStart()
	{
		Show(true);
		
		if (m_PauseButton != null)
			m_PauseButton.Restore();
	}

	void RegisterLevelRestart()
	{
		if (m_PauseButton != null)
			m_PauseButton.Resume();
	}

	void RegisterLevelExit()
	{
		Hide();
	}

	void RegisterAudioSourceChanged()
	{
		if (!Shown)
			return;
		
		if (m_PauseButton != null)
			m_PauseButton.Pause();
	}

	void RegisterAudioPlaySignal()
	{
		if (!Shown)
			return;
		
		if (m_PauseButton != null)
			m_PauseButton.Pause();
	}

	void RegisterAudioPauseSignal()
	{
		if (!Shown)
			return;
		
		if (m_PauseButton != null)
			m_PauseButton.Pause();
	}

	void OnApplicationPause(bool _Paused)
	{
		if (!Shown || !_Paused)
			return;
		
		if (m_PauseButton != null)
			m_PauseButton.Pause();
	}
}