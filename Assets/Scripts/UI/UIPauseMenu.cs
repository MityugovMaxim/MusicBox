using System;
using UnityEngine;
using Zenject;

public class UIPauseMenu : UIMenu, IInitializable, IDisposable
{
	const int RESTART_ADS_COUNT = 2;
	const int LEAVE_ADS_COUNT   = 3;

	[SerializeField] UILevelPreviewThumbnail m_Thumbnail;

	SignalBus      m_SignalBus;
	MenuProcessor  m_MenuProcessor;
	LevelProcessor m_LevelProcessor;
	AdsProcessor   m_AdsProcessor;

	int m_RestartAdsCount;
	int m_LeaveAdsCount;

	[Inject]
	public void Construct(
		SignalBus      _SignalBus,
		MenuProcessor  _MenuProcessor,
		LevelProcessor _LevelProcessor,
		AdsProcessor   _AdsProcessor
	)
	{
		m_SignalBus      = _SignalBus;
		m_MenuProcessor  = _MenuProcessor;
		m_LevelProcessor = _LevelProcessor;
		m_AdsProcessor   = _AdsProcessor;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
	}

	void RegisterLevelStart(LevelStartSignal _Signal)
	{
		Hide(true);
		
		m_Thumbnail.Setup(_Signal.LevelID);
	}

	public void Pause()
	{
		if (m_LevelProcessor != null)
			m_LevelProcessor.Pause();
		
		Show();
	}

	public void Resume()
	{
		if (m_LevelProcessor != null)
			m_LevelProcessor.Play();
		
		Hide();
	}

	public void Restart()
	{
		void RestartInternal()
		{
			if (m_LevelProcessor == null)
			{
				Debug.LogError("[UIPauseMenu] Restart level failed. Level provider is null.", gameObject);
				return;
			}
			
			m_LevelProcessor.Restart();
			
			CloseAction = m_LevelProcessor.Play;
			
			Hide();
		}
		
		m_RestartAdsCount++;
		
		if (m_RestartAdsCount >= RESTART_ADS_COUNT)
		{
			m_RestartAdsCount = 0;
			
			m_AdsProcessor.ShowInterstitial(RestartInternal);
		}
		else
		{
			RestartInternal();
		}
	}

	public void Leave()
	{
		void LeaveInternal()
		{
			if (m_LevelProcessor == null)
			{
				Debug.LogError("[UIPauseMenu] Leave level failed. Level provider is null.", gameObject);
				return;
			}
			
			m_LevelProcessor.Remove();
			
			m_MenuProcessor.Show(MenuType.MainMenu)
				.ThenHide(MenuType.GameMenu, true)
				.ThenHide(MenuType.PauseMenu, true);
		}
		
		m_LeaveAdsCount++;
		
		if (m_LeaveAdsCount >= LEAVE_ADS_COUNT)
		{
			m_LeaveAdsCount = 0;
			
			m_AdsProcessor.ShowInterstitial(LeaveInternal);
		}
		else
		{
			LeaveInternal();
		}
	}
}
