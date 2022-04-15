using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Functions;
using UnityEngine;
using Zenject;

[Menu(MenuType.SongMenu)]
public class UISongMenu : UISlideMenu
{
	const int PLAY_ADS_COUNT = 4;

	[SerializeField] UISongBackground m_Background;
	[SerializeField] UISongImage      m_Image;
	[SerializeField] UISongDiscs      m_Discs;
	[SerializeField] UISongLabel      m_Label;
	[SerializeField] UISongPrice      m_Price;
	[SerializeField] UISongMode       m_Play;
	[SerializeField] SongPreview      m_PreviewSource;
	[SerializeField] UIGroup          m_PlayGroup;
	[SerializeField] UIGroup          m_UnlockGroup;
	[SerializeField] UIGroup          m_LoaderGroup;
	[SerializeField] UIGroup          m_CompleteGroup;

	[Inject] SignalBus          m_SignalBus;
	[Inject] ProfileProcessor   m_ProfileProcessor;
	[Inject] SongsProcessor     m_SongsProcessor;
	[Inject] SongsManager       m_SongsManager;
	[Inject] AdsProcessor       m_AdsProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] HapticProcessor    m_HapticProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	string m_SongID;
	int    m_PlayAdsCount;

	public void Next()
	{
		m_StatisticProcessor.LogSongMenuNextClick(m_SongID);
		
		Select(GetLevelID(1));
	}

	public void Previous()
	{
		m_StatisticProcessor.LogSongMenuPreviousClick(m_SongID);
		
		Select(GetLevelID(-1));
	}

	public async void Unlock()
	{
		if (!m_SongsManager.IsSongLockedByCoins(m_SongID))
			return;
		
		m_StatisticProcessor.LogSongMenuUnlockClick(m_SongID);
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await Task.WhenAll(
			m_UnlockGroup.HideAsync(),
			m_LoaderGroup.ShowAsync()
		);
		
		bool success = await UnlockLevel(m_SongID);
		
		if (success)
		{
			m_StatisticProcessor.LogSongMenuUnlockSuccess(m_SongID);
			
			await m_LoaderGroup.HideAsync();
			
			m_HapticProcessor.Process(Haptic.Type.Success);
			
			await m_CompleteGroup.ShowAsync();
			
			await Task.WhenAll(
				m_ProfileProcessor.LoadProfile(),
				Task.Delay(1500)
			);
			
			m_PlayGroup.Show();
			m_CompleteGroup.Hide();
		}
		else
		{
			m_StatisticProcessor.LogSongMenuUnlockFailed(m_SongID);
			
			m_HapticProcessor.Process(Haptic.Type.Failure);
			
			await Task.WhenAll(
				m_UnlockGroup.ShowAsync(),
				m_CompleteGroup.HideAsync(),
				m_LoaderGroup.HideAsync(),
				m_PlayGroup.HideAsync()
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void Play()
	{
		if (!m_SongsManager.IsSongAvailable(m_SongID))
			return;
		
		m_StatisticProcessor.LogSongMenuPlayClick(m_SongID);
		
		m_PreviewSource.Stop();
		
		if (!await ProcessPlayAds())
		{
			m_PreviewSource.Play(m_SongID);
			return;
		}
		
		UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
		if (loadingMenu != null)
			loadingMenu.Setup(m_SongID);
		
		await m_MenuProcessor.Show(MenuType.LoadingMenu);
		
		await m_PlayGroup.ShowAsync(true);
		await m_LoaderGroup.HideAsync(true);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		await m_MenuProcessor.Hide(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.SongMenu, true);
		await m_MenuProcessor.Hide(MenuType.ProductMenu, true);
	}

	void RegisterScoreDataUpdate()
	{
		Select(m_SongID);
	}

	public void Setup(string _LevelID)
	{
		Select(_LevelID);
	}

	string GetLevelID(int _Offset)
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

	protected override void OnShowStarted()
	{
		m_SignalBus.Subscribe<ScoresDataUpdateSignal>(RegisterScoreDataUpdate);
	}

	protected override void OnShowFinished()
	{
		m_PreviewSource.Play(m_SongID);
	}

	protected override void OnHideStarted()
	{
		m_PreviewSource.Stop();
		
		m_SignalBus.Unsubscribe<ScoresDataUpdateSignal>(RegisterScoreDataUpdate);
	}

	void Select(string _LevelID)
	{
		m_SongID = _LevelID;
		
		m_Background.Setup(m_SongID, !Shown);
		m_Image.Setup(m_SongID);
		m_Discs.Setup(m_SongID);
		m_Label.Setup(m_SongID);
		m_Play.Setup(m_SongID);
		m_Price.Setup(m_SongID);
		
		if (m_SongsManager.IsSongLockedByCoins(m_SongID))
		{
			m_UnlockGroup.Show(true);
			m_PlayGroup.Hide(true);
		}
		else
		{
			m_PlayGroup.Show(true);
			m_UnlockGroup.Hide(true);
		}
		
		m_LoaderGroup.Hide(true);
		m_CompleteGroup.Hide(true);
		
		if (Shown)
			m_PreviewSource.Play(m_SongID);
	}

	async Task<bool> UnlockLevel(string _LevelID)
	{
		long coins = m_SongsProcessor.GetPrice(_LevelID);
		
		if (!await m_ProfileProcessor.CheckCoins(coins))
			return false;
		
		HttpsCallableReference unlockLevel = FirebaseFunctions.DefaultInstance.GetHttpsCallable("UnlockLevel");
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		data["level_id"] = _LevelID;
		
		bool success;
		
		try
		{
			HttpsCallableResult result = await unlockLevel.CallAsync(data);
			
			success = (bool)result.Data;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			
			success = false;
		}
		
		return success;
	}

	async Task<bool> ProcessPlayAds()
	{
		if (m_ProfileProcessor.HasNoAds())
			return true;
		
		SongMode songMode = m_SongsProcessor.GetMode(m_SongID);
		
		if (songMode == SongMode.Ads)
		{
			await m_MenuProcessor.Show(MenuType.BlockMenu, true);
			
			m_PlayGroup.Hide();
			m_LoaderGroup.Show();
			
			bool success = await m_AdsProcessor.Rewarded();
			
			await Task.Delay(500);
			
			if (success)
				return true;
			
			Debug.LogErrorFormat("[UILevelMenu] Play failed. Rewarded video error occured. Level ID: {0}.", m_SongID);
			
			m_PlayGroup.Show();
			m_LoaderGroup.Hide();
			
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
			
			return false;
		}
		else
		{
			m_PlayAdsCount++;
			
			if (m_PlayAdsCount < PLAY_ADS_COUNT)
				return true;
			
			m_PlayAdsCount = 0;
			
			m_PlayGroup.Hide();
			m_LoaderGroup.Show();
			
			await m_MenuProcessor.Show(MenuType.BlockMenu, true);
			
			await m_AdsProcessor.Interstitial();
			
			await Task.Delay(500);
			
			m_PlayGroup.Hide();
			m_LoaderGroup.Show();
			
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
			
			return true;
		}
	}
}