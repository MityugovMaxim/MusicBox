using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Functions;
using TMPro;
using UnityEngine;
using Zenject;

[Menu(MenuType.LevelMenu)]
public class UILevelMenu : UISlideMenu, IInitializable, IDisposable
{
	[SerializeField] UILevelBackground m_Background;
	[SerializeField] UILevelThumbnail  m_Thumbnail;
	[SerializeField] UILevelDiscs      m_Discs;
	[SerializeField] UILevelLabel      m_Label;
	[SerializeField] UILevelModeButton m_PlayButton;
	[SerializeField] UIGroup           m_PlayGroup;
	[SerializeField] UIGroup           m_UnlockGroup;
	[SerializeField] UIGroup           m_LoaderGroup;
	[SerializeField] UIGroup           m_CompleteGroup;
	[SerializeField] TMP_Text          m_PriceLabel;
	[SerializeField] UILoader          m_Loader;
	[SerializeField] LevelPreview      m_PreviewSource;

	SignalBus          m_SignalBus;
	ProfileProcessor   m_ProfileProcessor;
	LevelProcessor     m_LevelProcessor;
	LevelManager       m_LevelManager;
	AdsProcessor       m_AdsProcessor;
	MenuProcessor      m_MenuProcessor;
	ScoreProcessor     m_ScoreProcessor;
	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;

	string      m_LevelID;
	AudioSource m_AudioSource;

	[Inject]
	public void Construct(
		SignalBus          _SignalBus,
		LevelProcessor     _LevelProcessor,
		LevelManager       _LevelManager,
		AdsProcessor       _AdsProcessor,
		MenuProcessor      _MenuProcessor,
		ProfileProcessor   _ProfileProcessor,
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_SignalBus          = _SignalBus;
		m_LevelProcessor     = _LevelProcessor;
		m_LevelManager       = _LevelManager;
		m_AdsProcessor       = _AdsProcessor;
		m_MenuProcessor      = _MenuProcessor;
		m_ProfileProcessor   = _ProfileProcessor;
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
	}

	public void Next()
	{
		m_StatisticProcessor.LogLevelMenuNextClick(m_LevelID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		Select(GetLevelID(1));
	}

	public void Previous()
	{
		m_StatisticProcessor.LogLevelMenuPreviousClick(m_LevelID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		Select(GetLevelID(-1));
	}

	public async void Unlock()
	{
		// TODO: Uncomment
		//if (!m_LevelManager.IsLevelLockedByCoins(m_LevelID))
		//	return;
		
		m_StatisticProcessor.LogLevelMenuUnlockClick(m_LevelID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await Task.WhenAll(
			m_UnlockGroup.HideAsync(),
			m_LoaderGroup.ShowAsync()
		);
		
		// TODO: Uncomment
		// bool success = await UnlockLevel(m_LevelID);
		
		// TODO: Remove
		bool success = true;
		
		#if UNITY_EDITOR
		await Task.Delay(2500);
		#endif
		
		if (success)
		{
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
		if (!m_LevelManager.IsLevelAvailable(m_LevelID))
			return;
		
		m_StatisticProcessor.LogLevelMenuPlayClick(m_LevelID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		LevelMode levelMode = m_LevelProcessor.GetMode(m_LevelID);
		
		m_PreviewSource.Stop();
		
		if (levelMode == LevelMode.Ads)
		{
			await m_MenuProcessor.Show(MenuType.BlockMenu);
			
			m_PlayGroup.Hide();
			m_LoaderGroup.Show();
			
			m_Loader.Restore();
			
			#if UNITY_EDITOR
			await Task.Delay(5000);
			#endif
			
			bool success = await m_AdsProcessor.Rewarded();
			
			await Task.Delay(250);
			
			if (!success)
			{
				Debug.LogErrorFormat("[UILevelMenu] Play failed. Rewarded video error occured. Level ID: {0}.", m_LevelID);
				
				Setup(m_LevelID);
				
				m_PlayGroup.Show();
				m_LoaderGroup.Hide();
				
				await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
				
				return;
			}
		}
		
		UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
		if (loadingMenu != null)
			loadingMenu.Setup(m_LevelID);
		
		await m_MenuProcessor.Show(MenuType.LoadingMenu);
		
		await m_PlayGroup.ShowAsync(true);
		await m_LoaderGroup.HideAsync(true);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		await m_MenuProcessor.Hide(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.LevelMenu, true);
		await m_MenuProcessor.Hide(MenuType.ProductMenu, true);
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<AudioNextTrackSignal>(RegisterAudioNextTrack);
		m_SignalBus.Subscribe<AudioPreviousTrackSignal>(RegisterAudioPreviousTrack);
		m_SignalBus.Subscribe<ScoreDataUpdateSignal>(RegisterScoreDataUpdate);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<AudioNextTrackSignal>(RegisterAudioNextTrack);
		m_SignalBus.Unsubscribe<AudioPreviousTrackSignal>(RegisterAudioPreviousTrack);
		m_SignalBus.Unsubscribe<ScoreDataUpdateSignal>(RegisterScoreDataUpdate);
	}

	void RegisterAudioNextTrack()
	{
		if (Shown)
			Next();
	}

	void RegisterAudioPreviousTrack()
	{
		if (Shown)
			Previous();
	}

	void RegisterScoreDataUpdate()
	{
		if (Shown)
			Select(m_LevelID);
	}

	public void Setup(string _LevelID)
	{
		Select(_LevelID);
	}

	string GetLevelID(int _Offset)
	{
		List<string> levelIDs = m_LevelManager.GetLibraryLevelIDs();
		
		int index = levelIDs.IndexOf(m_LevelID);
		if (index >= 0 && index < levelIDs.Count)
			return levelIDs[MathUtility.Repeat(index + _Offset, levelIDs.Count)];
		else if (levelIDs.Count > 0)
			return levelIDs.FirstOrDefault();
		else
			return m_LevelID;
	}

	protected override void OnShowFinished()
	{
		m_PreviewSource.Play(m_LevelID);
	}

	protected override void OnHideStarted()
	{
		m_PreviewSource.Stop();
	}

	void Select(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		m_Background.Setup(m_LevelID, !Shown);
		m_Thumbnail.Setup(m_LevelID);
		m_Discs.Setup(m_LevelID);
		m_Label.Setup(m_LevelID);
		m_PlayButton.Setup(m_LevelID);
		
		m_PriceLabel.text = m_LevelProcessor.GetPrice(m_LevelID).ToString();
		
		// TODO: Remote 'true' from condition
		if (true || m_LevelManager.IsLevelLockedByCoins(m_LevelID))
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
			m_PreviewSource.Play(m_LevelID);
	}

	async Task<bool> UnlockLevel(string _LevelID)
	{
		long coins = m_LevelProcessor.GetPrice(_LevelID);
		
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
}