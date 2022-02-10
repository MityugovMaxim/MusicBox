using System;
using UnityEngine;
using Zenject;

[Menu(MenuType.GameMenu)]
public class UIGameMenu : UIMenu, IInitializable, IDisposable
{
	[SerializeField] UIGamePauseButton m_PauseButton;
	[SerializeField] UIGameProgress    m_Progress;
	[SerializeField] UIGameTimer       m_Timer;

	SignalBus       m_SignalBus;
	LevelController m_LevelController;
	MenuProcessor   m_MenuProcessor;

	string m_LevelID;

	[Inject]
	public void Construct(
		SignalBus       _SignalBus,
		LevelController _LevelController,
		MenuProcessor   _MenuProcessor
	)
	{
		m_SignalBus       = _SignalBus;
		m_LevelController = _LevelController;
		m_MenuProcessor   = _MenuProcessor;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Subscribe<LevelFinishSignal>(RegisterLevelFinish);
		m_SignalBus.Subscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
		
		m_LevelController.AddSampleReceiver(m_Progress);
		m_LevelController.AddSampleReceiver(m_Timer);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Unsubscribe<LevelFinishSignal>(RegisterLevelFinish);
		m_SignalBus.Unsubscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
		
		m_LevelController.RemoveSampleReceiver(m_Progress);
		m_LevelController.RemoveSampleReceiver(m_Timer);
	}

	void RegisterLevelRestart()
	{
		if (Shown)
			m_PauseButton.Resume();
	}

	async void RegisterLevelFinish()
	{
		UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
		if (resultMenu != null)
			resultMenu.Setup(m_LevelID);
		
		await m_MenuProcessor.Show(MenuType.ResultMenu);
		
		m_LevelController.Pause();
		
		await m_MenuProcessor.Hide(MenuType.ReviveMenu, true);
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
	}

	void RegisterAudioSourceChanged()
	{
		if (Shown && m_LevelController.Playing)
			m_PauseButton.Pause();
	}

	void OnApplicationPause(bool _Paused)
	{
		if (Shown && m_LevelController.Playing && _Paused)
			m_PauseButton.Pause();
	}

	void OnApplicationFocus(bool _Focus)
	{
		if (Shown && m_LevelController.Playing && !_Focus)
			m_PauseButton.Pause();
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		m_PauseButton.Setup(m_LevelID);
	}

	protected override void OnHideFinished()
	{
		if (m_PauseButton != null)
			m_PauseButton.Restore();
	}
}