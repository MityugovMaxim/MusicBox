using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.iOS;
using Zenject;

public class UIResultControlPage : UIResultMenuPage
{
	const int RESTART_ADS_COUNT = 2;
	const int LEAVE_ADS_COUNT   = 3;
	const int NEXT_ADS_COUNT    = 2;
	const int RATE_US_COUNT     = 2;

	public override ResultMenuPageType Type => ResultMenuPageType.Control;

	[SerializeField] UISongImage     m_Image;
	[SerializeField] UISongDiscs     m_Discs;
	[SerializeField] UISongLabel     m_Label;
	[SerializeField] UISongRating    m_Rating;
	[SerializeField] UISongMode      m_Mode;
	[SerializeField] SongPreview     m_Preview;
	[SerializeField] UISongPlatforms m_Platforms;

	[Inject] ProfileProcessor   m_ProfileProcessor;
	[Inject] SongsManager       m_SongsManager;
	[Inject] SongsProcessor     m_SongsProcessor;
	[Inject] SongController     m_SongController;
	[Inject] AdsProcessor       m_AdsProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	int m_LeaveAdsCount;
	int m_NextAdsCount;
	int m_RestartAdsCount;
	int m_RateUsCount;

	string m_SongID;

	public override void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.Setup(m_SongID);
		m_Discs.Setup(m_SongID);
		m_Label.Setup(m_SongID);
		m_Rating.Setup(m_SongID);
		m_Mode.Setup(m_SongID);
		m_Platforms.Setup(m_SongID);
		
		m_Preview.Stop();
	}

	public override void Play() { }

	public async void Leave()
	{
		m_StatisticProcessor.LogResultMenuControlPageLeaveClick(m_SongID);
		
		await ProcessLeaveAds();
		
		m_Preview.Stop();
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Songs);
		
		m_SongController.Leave();
		
		await m_MenuProcessor.Show(MenuType.MainMenu);
		await m_MenuProcessor.Hide(MenuType.ResultMenu, true);
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
	}

	public async void Next()
	{
		m_StatisticProcessor.LogResultMenuControlPageNextClick(m_SongID);
		
		await ProcessNextAds();
		
		m_Preview.Stop();
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Songs);
		
		UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
		if (songMenu != null)
			songMenu.Setup(GetLevelID(1));
		
		m_SongController.Leave();
		
		await m_MenuProcessor.Show(MenuType.SongMenu);
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.ResultMenu, true);
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
	}

	public async void Restart()
	{
		m_StatisticProcessor.LogResultMenuControlPageRestartClick(m_SongID);
		
		if (!await ProcessRestartAds())
			return;
		
		m_SongController.Restart();
		
		await m_MenuProcessor.Show(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
		await m_MenuProcessor.Hide(MenuType.ResultMenu);
	}

	string GetLevelID(int _Offset)
	{
		List<string> levelIDs = m_SongsManager.GetLibrarySongIDs();
		
		int index = levelIDs.IndexOf(m_SongID);
		if (index >= 0 && index < levelIDs.Count)
			return levelIDs[MathUtility.Repeat(index + _Offset, levelIDs.Count)];
		else if (levelIDs.Count > 0)
			return levelIDs.FirstOrDefault();
		else return m_SongID;
	}

	protected override void OnShowFinished()
	{
		m_RateUsCount++;
		
		if (m_RateUsCount >= RATE_US_COUNT)
		{
			m_RateUsCount = 0;
			
			Device.RequestStoreReview();
		}
		
		m_Preview.Play(m_SongID);
	}

	protected override void OnHideFinished()
	{
		m_Rating.Execute();
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

	async Task ProcessNextAds()
	{
		if (m_ProfileProcessor.HasNoAds())
			return;
		
		m_NextAdsCount++;
		
		if (m_NextAdsCount < NEXT_ADS_COUNT)
			return;
		
		m_NextAdsCount = 0;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_AdsProcessor.Interstitial();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
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