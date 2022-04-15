using System;
using UnityEngine;
using Zenject;

[Menu(MenuType.GameMenu)]
public class UIGameMenu : UIMenu, IInitializable, IDisposable
{
	[SerializeField] UISongLabel m_Label;

	[Inject] SignalBus      m_SignalBus;
	[Inject] SongController m_SongController;
	[Inject] MenuProcessor  m_MenuProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Label.Setup(m_SongID);
	}

	public async void Pause()
	{
		m_SongController.Pause();
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.PauseMenu);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	void RegisterAudioSourceChanged()
	{
		// if (Shown)
		// 	Pause();
	}

	void IInitializable.Initialize()
	{
		//m_SignalBus.Subscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
	}

	void IDisposable.Dispose()
	{
		//m_SignalBus.Unsubscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
	}
}