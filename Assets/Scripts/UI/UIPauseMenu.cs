﻿using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.PauseMenu)]
public class UIPauseMenu : UIMenu
{
	const int RESTART_ADS_COUNT = 2;
	const int LEAVE_ADS_COUNT   = 3;

	[SerializeField] UISongImage   m_Image;
	[SerializeField] UISongLabel   m_Label;
	[SerializeField] UIHapticState m_HapticState;

	[Inject] ProfileProcessor   m_ProfileProcessor;
	[Inject] SongsProcessor     m_SongsProcessor;
	[Inject] SongController     m_SongController;
	[Inject] AdsProcessor       m_AdsProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	string m_SongID;
	int    m_RestartAdsCount;
	int    m_LeaveAdsCount;

	public void Setup(string _LevelID)
	{
		m_SongID = _LevelID;
		
		m_Image.Setup(m_SongID);
		m_Label.Setup(m_SongID);
		
		m_HapticState.Setup();
	}

	public async void Restart()
	{
		m_StatisticProcessor.LogPauseMenuRestartClick(m_SongID);
		
		if (!await ProcessRestartAds())
			return;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		m_SongController.Restart();
		
		await m_MenuProcessor.Hide(MenuType.PauseMenu);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void Leave()
	{
		m_StatisticProcessor.LogPauseMenuLeaveClick(m_SongID);
		
		await ProcessLeaveAds();
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Songs);
		
		UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
		if (songMenu != null)
			songMenu.Setup(m_SongID);
		
		m_SongController.Leave();
		
		await m_MenuProcessor.Show(MenuType.SongMenu);
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
	}

	public async void Resume()
	{
		// TODO: Statistics
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		m_SongController.Resume();
		
		await m_MenuProcessor.Hide(MenuType.PauseMenu);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void Latency()
	{
		m_StatisticProcessor.LogPauseMenuLatencyClick(m_SongID);
		
		await m_MenuProcessor.Show(MenuType.LatencyMenu);
	}

	protected override void OnHideStarted()
	{
		if (m_HapticState != null)
			m_HapticState.Execute();
	}

	async Task<bool> ProcessRestartAds()
	{
		if (m_ProfileProcessor.HasNoAds())
			return true;
		
		LevelMode levelMode = m_SongsProcessor.GetMode(m_SongID);
		
		if (levelMode == LevelMode.Ads)
		{
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			bool success = await m_AdsProcessor.Rewarded();
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
			
			return success;
		}
		else
		{
			m_RestartAdsCount++;
			
			if (m_RestartAdsCount < RESTART_ADS_COUNT)
				return true;
			
			m_RestartAdsCount = 0;
			
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			await m_AdsProcessor.Interstitial();
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
			
			return true;
		}
	}

	async Task ProcessLeaveAds()
	{
		if (m_ProfileProcessor.HasNoAds())
			return;
		
		m_LeaveAdsCount++;
		
		if (m_LeaveAdsCount < LEAVE_ADS_COUNT)
			return;
		
		m_LeaveAdsCount = 0;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_AdsProcessor.Interstitial();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}
}
