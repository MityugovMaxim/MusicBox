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

	AdsProcessor     m_AdsProcessor;
	ProfileProcessor m_ProfileProcessor;
	LevelProcessor   m_LevelProcessor;
	HealthProcessor  m_HealthProcessor;
	MenuProcessor    m_MenuProcessor;
	HapticProcessor  m_HapticProcessor;

	string m_LevelID;
	int    m_ReviveCount;
	int    m_RestartAdsCount;
	int    m_LeaveAdsCount;

	[Inject]
	public void Construct(
		AdsProcessor     _AdsProcessor,
		ProfileProcessor _ProfileProcessor,
		LevelProcessor   _LevelProcessor,
		HealthProcessor  _HealthProcessor,
		MenuProcessor    _MenuProcessor,
		HapticProcessor  _HapticProcessor
	)
	{
		m_AdsProcessor     = _AdsProcessor;
		m_ProfileProcessor = _ProfileProcessor;
		m_LevelProcessor   = _LevelProcessor;
		m_HealthProcessor  = _HealthProcessor;
		m_MenuProcessor    = _MenuProcessor;
		m_HapticProcessor  = _HapticProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID     = _LevelID;
		m_ReviveCount = 0;
		m_LevelThumbnail.Setup(m_LevelID);
		m_RestartButton.Setup(m_LevelID);
	}

	public async void ReviveCoins()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		bool success = await ReviveLevel(m_LevelID, m_ReviveCount);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		
		if (success)
		{
			m_ReviveCount++;
			
			await m_MenuProcessor.Hide(MenuType.ReviveMenu);
			
			await Task.Delay(1000);
			
			m_HealthProcessor.Restore(2);
			
			m_LevelProcessor.Play();
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void ReviveAds()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		bool success = await m_AdsProcessor.Rewarded(true);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		
		if (success)
		{
			m_ReviveCount++;
			
			await m_MenuProcessor.Hide(MenuType.ReviveMenu);
			
			await Task.Delay(1000);
			
			m_HealthProcessor.Restore(2);
			
			m_LevelProcessor.Play();
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void Restart()
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		LevelMode levelMode = m_LevelProcessor.GetMode(m_LevelID);
		
		if (levelMode == LevelMode.Ads)
		{
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			await m_AdsProcessor.Rewarded();
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		}
		else
		{
			m_RestartAdsCount++;
			
			if (m_RestartAdsCount >= RESTART_ADS_COUNT)
			{
				m_RestartAdsCount = 0;
				
				await m_MenuProcessor.Show(MenuType.ProcessingMenu);
				
				await m_AdsProcessor.Interstitial();
				
				await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
			}
		}
		
		m_LevelProcessor.Restart();
		
		await m_MenuProcessor.Show(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
		await m_MenuProcessor.Hide(MenuType.ReviveMenu, true);
		await m_MenuProcessor.Hide(MenuType.ResultMenu);
		
		m_LevelProcessor.Play();
	}

	public async void Leave()
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		m_LeaveAdsCount++;
		
		if (m_LeaveAdsCount >= LEAVE_ADS_COUNT)
		{
			m_LeaveAdsCount = 0;
			
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			await m_AdsProcessor.Interstitial();
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		}
		
		m_LevelProcessor.Remove();
		
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
}