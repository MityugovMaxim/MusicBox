using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIResultControlPage : UIResultMenuPage
{
	public override ResultMenuPageType Type => ResultMenuPageType.Control;

	[SerializeField] UISongImage     m_Image;
	[SerializeField] UISongDiscs     m_Discs;
	[SerializeField] UISongLabel     m_Label;
	[SerializeField] UISongRating    m_Rating;
	[SerializeField] UISongMode      m_Mode;
	[SerializeField] SongPreview     m_Preview;
	[SerializeField] UISongPlatforms m_Platforms;

	[Inject] ProfileProcessor   m_ProfileProcessor;
	[Inject] ConfigProcessor    m_ConfigProcessor;
	[Inject] SongsManager       m_SongsManager;
	[Inject] SongsProcessor     m_SongsProcessor;
	[Inject] SongController     m_SongController;
	[Inject] AdsProcessor       m_AdsProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	int m_LeaveAdsCount;
	int m_NextAdsCount;
	int m_RestartAdsCount;
	int m_ReviewRequestCount;

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
			songMenu.Setup(GetSongID(1));
		
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

	string GetSongID(int _Offset)
	{
		List<string> songIDs = m_SongsManager.GetLibrarySongIDs();
		
		int index = songIDs.IndexOf(m_SongID);
		if (index >= 0 && index < songIDs.Count)
			return songIDs[MathUtility.Repeat(index + _Offset, songIDs.Count)];
		else if (songIDs.Count > 0)
			return songIDs.FirstOrDefault();
		else
			return m_SongID;
	}

	protected override void OnShowFinished()
	{
		m_ReviewRequestCount++;
		
		if (m_ReviewRequestCount == m_ConfigProcessor.ReviewRequestCount)
		{
			#if UNITY_IOS
			UnityEngine.iOS.Device.RequestStoreReview();
			#elif UNITY_ANDROID
			// TODO: Create review request
			Log.Info(this, "Request store review");
			#endif
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

	async Task ProcessNextAds()
	{
		if (m_ProfileProcessor.HasNoAds())
			return;
		
		m_NextAdsCount++;
		
		if (m_NextAdsCount < m_ConfigProcessor.SongNextAdsCount)
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
		
		if (m_LeaveAdsCount < m_ConfigProcessor.SongLeaveAdsCount)
			return;
		
		m_LeaveAdsCount = 0;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_AdsProcessor.Interstitial();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}
}