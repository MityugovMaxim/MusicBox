using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.PauseMenu)]
public class UIPauseMenu : UIMenu
{
	const int RESTART_ADS_COUNT = 2;
	const int LEAVE_ADS_COUNT   = 3;

	[SerializeField] UILevelThumbnail m_Thumbnail;
	[SerializeField] UIHapticState    m_HapticState;

	ProfileProcessor   m_ProfileProcessor;
	LevelProcessor     m_LevelProcessor;
	LevelController    m_LevelController;
	AdsProcessor       m_AdsProcessor;
	MenuProcessor      m_MenuProcessor;
	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;

	string m_LevelID;
	int    m_RestartAdsCount;
	int    m_LeaveAdsCount;

	[Inject]
	public void Construct(
		ProfileProcessor   _ProfileProcessor,
		LevelProcessor     _LevelProcessor,
		LevelController    _LevelController,
		AdsProcessor       _AdsProcessor,
		MenuProcessor      _MenuProcessor,
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_ProfileProcessor   = _ProfileProcessor;
		m_MenuProcessor      = _MenuProcessor;
		m_LevelProcessor     = _LevelProcessor;
		m_LevelController    = _LevelController;
		m_AdsProcessor       = _AdsProcessor;
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		m_Thumbnail.Setup(m_LevelID);
		
		m_HapticState.Setup();
	}

	public async void Restart()
	{
		m_StatisticProcessor.LogPauseMenuRestartClick(m_LevelID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		if (!await ProcessRestartAds())
			return;
		
		m_LevelController.Restart();
		
		await m_MenuProcessor.Hide(MenuType.PauseMenu);
		
		m_LevelController.Play();
	}

	public async void Leave()
	{
		m_StatisticProcessor.LogPauseMenuLeaveClick(m_LevelID);
		
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
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
	}

	public void Latency()
	{
		m_StatisticProcessor.LogPauseMenuLatencyClick(m_LevelID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		UILatencyMenu latencyMenu = m_MenuProcessor.GetMenu<UILatencyMenu>();
		
		if (latencyMenu != null)
			latencyMenu.Setup(m_LevelID);
		
		m_MenuProcessor.Show(MenuType.LatencyMenu);
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
