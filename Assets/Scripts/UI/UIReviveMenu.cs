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
		MenuProcessor    _MenuProcessor,
		HapticProcessor  _HapticProcessor
	)
	{
		m_AdsProcessor     = _AdsProcessor;
		m_ProfileProcessor = _ProfileProcessor;
		m_LevelProcessor   = _LevelProcessor;
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
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		bool success = await m_ProfileProcessor.ReviveLevel(m_LevelID, m_ReviveCount);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		
		if (!success)
			return;
		
		m_ReviveCount++;
		
		await m_MenuProcessor.Hide(MenuType.ReviveMenu);
		
		m_LevelProcessor.Revive();
	}

	public async void ReviveAds()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		bool success = await m_AdsProcessor.Rewarded(true);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		
		if (!success)
			return;
		
		m_ReviveCount++;
		
		await m_MenuProcessor.Hide(MenuType.ReviveMenu);
		
		m_LevelProcessor.Revive();
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
}