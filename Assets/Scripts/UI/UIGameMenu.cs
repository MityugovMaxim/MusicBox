using System;
using UnityEngine;
using Zenject;

public class UIGameMenu : UIMenu, IInitializable, IDisposable
{
	[SerializeField] UIGamePauseButton m_PauseButton;
	[SerializeField] UIGameProgress    m_Progress;
	[SerializeField] UIGameTimer       m_Timer;
	[SerializeField] UITrackInfo       m_TrackInfo;
	[SerializeField] LevelInfo         m_TutorialInfo;

	SignalBus      m_SignalBus;
	LevelProcessor m_LevelProcessor;

	[Inject]
	public void Construct(SignalBus _SignalBus, LevelProcessor _LevelProcessor)
	{
		m_SignalBus      = _SignalBus;
		m_LevelProcessor = _LevelProcessor;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Subscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
		m_SignalBus.Subscribe<AudioPlaySignal>(RegisterAudioPlaySignal);
		m_SignalBus.Subscribe<AudioPauseSignal>(RegisterAudioPauseSignal);
		
		m_LevelProcessor.AddSampleReceiver(m_Progress);
		m_LevelProcessor.AddSampleReceiver(m_Timer);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Unsubscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
		m_SignalBus.Unsubscribe<AudioPlaySignal>(RegisterAudioPlaySignal);
		m_SignalBus.Unsubscribe<AudioPauseSignal>(RegisterAudioPauseSignal);
		
		m_LevelProcessor.RemoveSampleReceiver(m_Progress);
		m_LevelProcessor.RemoveSampleReceiver(m_Timer);
	}

	void RegisterLevelRestart()
	{
		if (m_PauseButton != null)
			m_PauseButton.Resume();
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

	public void Setup(string _LevelID)
	{
		bool tutorial = m_TutorialInfo != null && m_TutorialInfo.ID == _LevelID;
		
		if (m_Progress != null)
			m_Progress.gameObject.SetActive(!tutorial);
		
		if (m_Timer != null)
			m_Timer.gameObject.SetActive(!tutorial);
		
		if (m_TrackInfo != null)
			m_TrackInfo.gameObject.SetActive(!tutorial);
	}

	protected override void OnHideFinished()
	{
		if (m_PauseButton != null)
			m_PauseButton.Restore();
	}
}