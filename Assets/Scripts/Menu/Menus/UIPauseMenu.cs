using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.PauseMenu)]
public class UIPauseMenu : UIMenu
{
	[SerializeField] UISongImage m_Image;
	[SerializeField] UISongLabel m_Label;

	[Inject] ProfileProcessor   m_ProfileProcessor;
	[Inject] ConfigProcessor    m_ConfigProcessor;
	[Inject] SongsProcessor     m_SongsProcessor;
	[Inject] SongController     m_SongController;
	[Inject] AdsProcessor       m_AdsProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	string m_SongID;
	int    m_RestartAdsCount;
	int    m_LeaveAdsCount;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.Setup(m_SongID);
		m_Label.Setup(m_SongID);
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

	async Task<bool> ProcessRestartAds()
	{
		if (m_ProfileProcessor.HasNoAds())
			return true;
		
		SongMode songMode = m_SongsProcessor.GetMode(m_SongID);
		
		if (songMode == SongMode.Ads)
		{
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			bool success = await m_AdsProcessor.Rewarded();
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
			
			return success;
		}
		else
		{
			m_RestartAdsCount++;
			
			if (m_RestartAdsCount < m_ConfigProcessor.SongRestartAdsCount)
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
		
		if (m_LeaveAdsCount < m_ConfigProcessor.SongLeaveAdsCount)
			return;
		
		m_LeaveAdsCount = 0;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_AdsProcessor.Interstitial();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}
}
