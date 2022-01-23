using UnityEngine;
using Zenject;

[Menu(MenuType.PauseMenu)]
public class UIPauseMenu : UIMenu
{
	const int RESTART_ADS_COUNT = 2;
	const int LEAVE_ADS_COUNT   = 3;

	[SerializeField] UILevelThumbnail m_Thumbnail;
	[SerializeField] UIHapticState    m_HapticState;

	MenuProcessor   m_MenuProcessor;
	LevelProcessor  m_LevelProcessor;
	AdsProcessor    m_AdsProcessor;
	HapticProcessor m_HapticProcessor;

	string m_LevelID;
	int    m_RestartAdsCount;
	int    m_LeaveAdsCount;

	[Inject]
	public void Construct(
		MenuProcessor   _MenuProcessor,
		LevelProcessor  _LevelProcessor,
		AdsProcessor    _AdsProcessor,
		HapticProcessor _HapticProcessor
	)
	{
		m_MenuProcessor   = _MenuProcessor;
		m_LevelProcessor  = _LevelProcessor;
		m_AdsProcessor    = _AdsProcessor;
		m_HapticProcessor = _HapticProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		m_Thumbnail.Setup(m_LevelID);
		
		m_HapticState.Setup();
	}

	public async void Restart()
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		m_RestartAdsCount++;
		
		if (m_RestartAdsCount >= RESTART_ADS_COUNT)
		{
			m_RestartAdsCount = 0;
			
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			await m_AdsProcessor.Interstitial();
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		}
		
		m_LevelProcessor.Restart();
		
		await m_MenuProcessor.Hide(MenuType.PauseMenu);
		
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
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
	}

	public void Latency()
	{
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
}
