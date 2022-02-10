using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Functions;
using UnityEngine;
using Zenject;

[Menu(MenuType.ReviveMenu)]
public class UIReviveMenu : UIMenu
{
	const int RESTART_ADS_COUNT = 2;
	const int LEAVE_ADS_COUNT   = 2;

	[SerializeField] UILevelThumbnail  m_LevelThumbnail;
	[SerializeField] UILevelModeButton m_RestartButton;

	AdsProcessor       m_AdsProcessor;
	ProfileProcessor   m_ProfileProcessor;
	LevelProcessor     m_LevelProcessor;
	LevelController    m_LevelController;
	HealthProcessor    m_HealthProcessor;
	MenuProcessor      m_MenuProcessor;
	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;

	string m_LevelID;
	int    m_ReviveCount;
	int    m_RestartAdsCount;
	int    m_LeaveAdsCount;

	[Inject]
	public void Construct(
		AdsProcessor       _AdsProcessor,
		ProfileProcessor   _ProfileProcessor,
		LevelProcessor     _LevelProcessor,
		LevelController    _LevelController,
		HealthProcessor    _HealthProcessor,
		MenuProcessor      _MenuProcessor,
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_AdsProcessor       = _AdsProcessor;
		m_ProfileProcessor   = _ProfileProcessor;
		m_LevelProcessor     = _LevelProcessor;
		m_LevelController    = _LevelController;
		m_HealthProcessor    = _HealthProcessor;
		m_MenuProcessor      = _MenuProcessor;
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID     = _LevelID;
		m_ReviveCount = 0;
		m_LevelThumbnail.Setup(m_LevelID);
		m_RestartButton.Setup(m_LevelID);
		
		m_StatisticProcessor.LogReviveMenuShow(m_LevelID);
	}

	public async void ReviveCoins()
	{
		m_StatisticProcessor.LogReviveMenuReviveCoinsClick(m_LevelID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		bool success = await ReviveLevel(m_LevelID, m_ReviveCount);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		
		if (success)
		{
			m_ReviveCount++;
			
			await m_MenuProcessor.Hide(MenuType.ReviveMenu);
			
			await Task.Delay(1000);
			
			m_HealthProcessor.Restore();
			
			m_LevelController.Play();
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void ReviveAds()
	{
		m_StatisticProcessor.LogReviveMenuReviveAdsClick(m_LevelID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		bool success = await m_AdsProcessor.Rewarded();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		
		if (success)
		{
			m_ReviveCount++;
			
			await m_MenuProcessor.Hide(MenuType.ReviveMenu);
			
			await Task.Delay(1000);
			
			m_HealthProcessor.Restore();
			
			m_LevelController.Play();
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void Restart()
	{
		m_StatisticProcessor.LogReviveMenuRestartClick(m_LevelID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		if (!await ProcessRestartAds())
			return;
		
		m_LevelController.Restart();
		
		await m_MenuProcessor.Show(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
		await m_MenuProcessor.Hide(MenuType.ReviveMenu, true);
		await m_MenuProcessor.Hide(MenuType.ResultMenu);
		
		m_LevelController.Play();
	}

	public async void Leave()
	{
		m_StatisticProcessor.LogReviveMenuLeaveClick(m_LevelID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		await ProcessLeaveAds();
		
		m_LevelController.Remove();
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Levels);
		
		UILevelMenu levelMenu = m_MenuProcessor.GetMenu<UILevelMenu>();
		if (levelMenu != null)
			levelMenu.Setup(m_LevelID);
		
		await m_MenuProcessor.Show(MenuType.LevelMenu);
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.ReviveMenu, true);
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
	}

	async Task<bool> ReviveLevel(string _LevelID, int _ReviveCount)
	{
		long coins = m_LevelProcessor.GetRevivePrice(_LevelID);
		
		if (!await m_ProfileProcessor.CheckCoins(coins))
			return false;
		
		HttpsCallableReference revive = FirebaseFunctions.DefaultInstance.GetHttpsCallable("ReviveLevel");
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		data["level_id"]     = _LevelID;
		data["revive_count"] = _ReviveCount;
		
		bool success;
		
		try
		{
			HttpsCallableResult result = await revive.CallAsync(data);
			
			success = (bool)result.Data;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			
			success = false;
		}
		
		return success;
	}

	async Task<bool> ProcessRestartAds()
	{
		if (m_ProfileProcessor.HasNoAds())
			return true;
		
		LevelMode levelMode = m_LevelProcessor.GetMode(m_LevelID);
		
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