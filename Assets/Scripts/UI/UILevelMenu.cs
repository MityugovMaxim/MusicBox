using System;
using UnityEngine;
using Zenject;

[Menu(MenuType.LevelMenu)]
public class UILevelMenu : UISlideMenu, IInitializable, IDisposable
{
	[SerializeField] UILevelBackground m_Background;
	[SerializeField] UILevelThumbnail  m_Thumbnail;
	[SerializeField] UILevelRanks             m_LevelRanks;
	[SerializeField] UILevelLabel      m_Label;
	[SerializeField] UILevelModeButton        m_PlayButton;
	[SerializeField] LevelPreviewAudioSource  m_PreviewSource;

	SignalBus        m_SignalBus;
	LevelProcessor   m_LevelProcessor;
	AdsProcessor     m_AdsProcessor;
	MenuProcessor    m_MenuProcessor;
	ProfileProcessor m_ProfileProcessor;
	HapticProcessor  m_HapticProcessor;

	string      m_LevelID;
	AudioSource m_AudioSource;

	[Inject]
	public void Construct(
		SignalBus         _SignalBus,
		LevelProcessor    _LevelProcessor,
		AdsProcessor      _AdsProcessor,
		MenuProcessor     _MenuProcessor,
		ProfileProcessor _ProfileProcessor,
		HapticProcessor   _HapticProcessor
	)
	{
		m_SignalBus         = _SignalBus;
		m_LevelProcessor    = _LevelProcessor;
		m_AdsProcessor      = _AdsProcessor;
		m_MenuProcessor     = _MenuProcessor;
		m_ProfileProcessor = _ProfileProcessor;
		m_HapticProcessor   = _HapticProcessor;
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
		string levelID = m_LevelProcessor.GetNextLevelID(m_LevelID);
		
		Select(levelID);
	}

	public void Previous()
	{
		string levelID = m_LevelProcessor.GetPreviousLevelID(m_LevelID);
		
		Select(levelID);
	}

	public async void Play()
	{
		if (m_ProfileProcessor.IsLevelLocked(m_LevelID))
			return;
		
		LevelMode levelMode = m_LevelProcessor.GetLevelMode(m_LevelID);
		
		m_PreviewSource.Stop();
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		if (levelMode == LevelMode.Ads)
		{
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			bool success = await m_AdsProcessor.ShowRewardedAsync(this);
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
			
			if (!success)
			{
				Debug.LogErrorFormat("[UILevelMenu] Play failed. Rewarded video error occured. Level ID: {0}.", m_LevelID);
				
				Setup(m_LevelID);
				
				return;
			}
		}
		
		UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
		if (loadingMenu != null)
			loadingMenu.Setup(m_LevelID);
		
		await m_MenuProcessor.Show(MenuType.LoadingMenu);
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
		m_LevelRanks.Setup(m_LevelID);
		m_Label.Setup(m_LevelID);
		m_PlayButton.Setup(m_LevelID);
		
		if (Shown)
			m_PreviewSource.Play(m_LevelID);
	}
}