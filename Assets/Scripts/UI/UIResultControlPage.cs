using System.Collections.Generic;
using System.Linq;
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

	[SerializeField] UILevelThumbnail  m_Thumbnail;
	[SerializeField] UILevelDiscs      m_Discs;
	[SerializeField] UILevelLabel      m_Label;
	[SerializeField] UILevelLikeButton m_LikeButton;
	[SerializeField] UILevelModeButton m_RestartButton;
	[SerializeField] LevelPreview      m_PreviewSource;

	LevelManager       m_LevelManager;
	LevelProcessor     m_LevelProcessor;
	AdsProcessor       m_AdsProcessor;
	MenuProcessor      m_MenuProcessor;
	AmbientProcessor   m_AmbientProcessor;
	MusicProcessor     m_MusicProcessor;
	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;
	UrlProcessor       m_UrlProcessor;

	int m_LeaveAdsCount;
	int m_NextAdsCount;
	int m_RestartAdsCount;
	int m_RateUsCount;

	string m_LevelID;

	[Inject]
	public void Construct(
		LevelManager       _LevelManager,
		ProfileProcessor   _ProfileProcessor,
		LevelProcessor     _LevelProcessor,
		AdsProcessor       _AdsProcessor,
		MenuProcessor      _MenuProcessor,
		AmbientProcessor   _AmbientProcessor,
		MusicProcessor     _MusicProcessor,
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor,
		UrlProcessor       _UrlProcessor
	)
	{
		m_LevelManager       = _LevelManager;
		m_LevelProcessor     = _LevelProcessor;
		m_AdsProcessor       = _AdsProcessor;
		m_MenuProcessor      = _MenuProcessor;
		m_AmbientProcessor   = _AmbientProcessor;
		m_MusicProcessor     = _MusicProcessor;
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
		m_UrlProcessor       = _UrlProcessor;
	}

	public override void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		m_Thumbnail.Setup(m_LevelID);
		m_Discs.Setup(m_LevelID);
		m_Label.Setup(m_LevelID);
		m_LikeButton.Setup(m_LevelID);
		m_RestartButton.Setup(m_LevelID);
		m_PreviewSource.Stop();
	}

	public override void Play() { }

	public async void Leave()
	{
		m_StatisticProcessor.LogResultMenuControlPageLeaveClick(m_LevelID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		m_PreviewSource.Stop();
		
		m_LeaveAdsCount++;
		
		if (m_LeaveAdsCount >= LEAVE_ADS_COUNT)
		{
			m_LeaveAdsCount = 0;
			
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			await m_AdsProcessor.Interstitial();
			
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		}
		
		m_LevelProcessor.Remove();
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Levels);
		
		await m_MenuProcessor.Show(MenuType.MainMenu);
		await m_MenuProcessor.Hide(MenuType.ResultMenu, true);
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
	}

	public async void Next()
	{
		m_StatisticProcessor.LogResultMenuControlPageNextClick(m_LevelID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		m_PreviewSource.Stop();
		
		m_NextAdsCount++;
		
		if (m_NextAdsCount >= NEXT_ADS_COUNT)
		{
			m_NextAdsCount = 0;
			
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
			levelMenu.Setup(GetLevelID(1));
		
		await m_MenuProcessor.Show(MenuType.LevelMenu);
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.ResultMenu, true);
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
	}

	public async void Restart()
	{
		m_StatisticProcessor.LogResultMenuControlPageRestartClick(m_LevelID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		m_AmbientProcessor.Pause();
		m_MusicProcessor.StopPreview();
		
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
		await m_MenuProcessor.Hide(MenuType.ResultMenu);
		
		m_LevelProcessor.Play();
	}

	public void OpenAppleMusic()
	{
		OpenPlatform("apple_music");
	}

	public void OpenSpotify()
	{
		OpenPlatform("spotify");
	}

	public void OpenDeezer()
	{
		OpenPlatform("deezer");
	}

	async void OpenPlatform(string _PlatformID)
	{
		m_StatisticProcessor.LogResultMenuControlPagePlatformClick(m_LevelID, _PlatformID);
		
		string url = m_LevelProcessor.GetPlatformURL(m_LevelID, _PlatformID);
		
		if (string.IsNullOrEmpty(url))
			return;
		
		await m_UrlProcessor.ProcessURL(url);
	}

	string GetLevelID(int _Offset)
	{
		List<string> levelIDs = m_LevelManager.GetLibraryLevelIDs();
		
		int index = levelIDs.IndexOf(m_LevelID);
		if (index >= 0 && index < levelIDs.Count)
			return levelIDs[MathUtility.Repeat(index + _Offset, levelIDs.Count)];
		else if (levelIDs.Count > 0)
			return levelIDs.FirstOrDefault();
		else return m_LevelID;
	}

	protected override void OnShowFinished()
	{
		m_RateUsCount++;
		
		if (m_RateUsCount >= RATE_US_COUNT)
		{
			m_RateUsCount = 0;
			
			Device.RequestStoreReview();
		}
		
		m_PreviewSource.Play(m_LevelID);
	}

	protected override void OnHideFinished()
	{
		m_LikeButton.Execute();
	}
}