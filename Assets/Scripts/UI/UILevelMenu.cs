using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Functions;
using UnityEngine;
using Zenject;

[Menu(MenuType.LevelMenu)]
public class UILevelMenu : UISlideMenu, IInitializable, IDisposable
{
	[SerializeField] UILevelBackground       m_Background;
	[SerializeField] UILevelThumbnail        m_Thumbnail;
	[SerializeField] UILevelDiscs            m_Discs;
	[SerializeField] UILevelLabel            m_Label;
	[SerializeField] UILevelModeButton       m_PlayButton;
	[SerializeField] UIGroup                 m_PlayGroup;
	[SerializeField] UIGroup                 m_UnlockGroup;
	[SerializeField] UIGroup                 m_LoaderGroup;
	[SerializeField] UILoader                m_Loader;
	[SerializeField] LevelPreviewAudioSource m_PreviewSource;

	SignalBus        m_SignalBus;
	ProfileProcessor m_ProfileProcessor;
	LevelProcessor   m_LevelProcessor;
	LevelManager     m_LevelManager;
	AdsProcessor     m_AdsProcessor;
	MenuProcessor    m_MenuProcessor;
	ScoreProcessor   m_ScoreProcessor;
	HapticProcessor  m_HapticProcessor;

	string      m_LevelID;
	AudioSource m_AudioSource;

	[Inject]
	public void Construct(
		SignalBus        _SignalBus,
		LevelProcessor   _LevelProcessor,
		LevelManager     _LevelManager,
		AdsProcessor     _AdsProcessor,
		MenuProcessor    _MenuProcessor,
		ProfileProcessor _ProfileProcessor,
		HapticProcessor  _HapticProcessor
	)
	{
		m_SignalBus        = _SignalBus;
		m_LevelProcessor   = _LevelProcessor;
		m_LevelManager     = _LevelManager;
		m_AdsProcessor     = _AdsProcessor;
		m_MenuProcessor    = _MenuProcessor;
		m_ProfileProcessor = _ProfileProcessor;
		m_HapticProcessor  = _HapticProcessor;
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

	public void Next()
	{
		Select(GetLevelID(1));
	}

	public void Previous()
	{
		Select(GetLevelID(-1));
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

	public async void Unlock()
	{
		if (!m_LevelManager.IsLevelLockedByCoins(m_LevelID))
			return;
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		bool success = await UnlockLevel(m_LevelID);
		
		if (success)
		{
			m_PlayGroup.Show();
			m_UnlockGroup.Hide();
			m_LoaderGroup.Hide();
		}
		else
		{
			m_PlayGroup.Hide();
			m_UnlockGroup.Show();
			m_LoaderGroup.Hide();
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void Play()
	{
		if (!m_LevelManager.IsLevelAvailable(m_LevelID))
			return;
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		LevelMode levelMode = m_LevelProcessor.GetMode(m_LevelID);
		
		m_PreviewSource.Stop();
		
		if (levelMode == LevelMode.Ads)
		{
			await m_MenuProcessor.Show(MenuType.BlockMenu);
			
			m_PlayGroup.Hide();
			m_LoaderGroup.Show();
			
			m_Loader.Restore();
			m_Loader.Play();
			
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
		
		if (m_LevelManager.IsLevelLockedByCoins(m_LevelID))
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