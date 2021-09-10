using System;
using UnityEngine;
using Zenject;

[Menu(MenuType.LevelMenu)]
public class UILevelMenu : UISlideMenu, IInitializable, IDisposable
{
	[SerializeField] UILevelPreviewBackground m_Background;
	[SerializeField] UILevelPreviewThumbnail  m_Thumbnail;
	[SerializeField] UIScoreRank              m_ScoreRank;
	[SerializeField] UILevelPreviewLabel      m_Label;
	[SerializeField] UILevelModeButton        m_PlayButton;
	[SerializeField] UILevelProgress          m_Progress;
	[SerializeField] GameObject               m_ExpPayout;
	[SerializeField] UIExpLabel               m_RankSExpPayout;
	[SerializeField] UIExpLabel               m_RankAExpPayout;
	[SerializeField] UIExpLabel               m_RankBExpPayout;
	[SerializeField] UIExpLabel               m_RankCExpPayout;
	[SerializeField] LevelPreviewAudioSource  m_PreviewSource;

	SignalBus         m_SignalBus;
	LevelProcessor    m_LevelProcessor;
	AdsProcessor      m_AdsProcessor;
	MenuProcessor     m_MenuProcessor;
	ProgressProcessor m_ProgressProcessor;
	HapticProcessor   m_HapticProcessor;

	string      m_LevelID;
	AudioSource m_AudioSource;

	[Inject]
	public void Construct(
		SignalBus         _SignalBus,
		LevelProcessor    _LevelProcessor,
		AdsProcessor      _AdsProcessor,
		MenuProcessor     _MenuProcessor,
		ProgressProcessor _ProgressProcessor,
		HapticProcessor   _HapticProcessor
	)
	{
		m_SignalBus         = _SignalBus;
		m_LevelProcessor    = _LevelProcessor;
		m_AdsProcessor      = _AdsProcessor;
		m_MenuProcessor     = _MenuProcessor;
		m_ProgressProcessor = _ProgressProcessor;
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

	public void Play()
	{
		if (m_ProgressProcessor.IsLevelLocked(m_LevelID))
			return;
		
		void PlayInternal()
		{
			UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
			if (loadingMenu != null)
				loadingMenu.Setup(m_LevelID);
			m_MenuProcessor.Show(MenuType.LoadingMenu);
		}
		
		LevelMode levelMode = m_LevelProcessor.GetLevelMode(m_LevelID);
		
		m_PreviewSource.Stop();
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		if (levelMode == LevelMode.Ads)
		{
			m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			m_AdsProcessor.ShowRewardedAsync(
				this,
				() =>
				{
					m_MenuProcessor.Hide(MenuType.ProcessingMenu, true);
					
					PlayInternal();
				},
				() =>
				{
					m_MenuProcessor.Hide(MenuType.ProcessingMenu, true);
					
					Setup(m_LevelID);
				},
				() =>
				{
					m_MenuProcessor.Hide(MenuType.ProcessingMenu);
					
					Setup(m_LevelID);
				}
			);
		}
		else
		{
			PlayInternal();
		}
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
		m_ScoreRank.Setup(m_LevelID);
		m_Label.Setup(m_LevelID);
		m_Progress.Setup(m_LevelID);
		m_PlayButton.Setup(m_LevelID);
		
		m_ExpPayout.SetActive(m_ProgressProcessor.IsLevelUnlocked(m_LevelID) && m_ProgressProcessor.GetExpPayout(m_LevelID) > 0);
		
		m_RankSExpPayout.Exp = m_ProgressProcessor.GetExpPayout(m_LevelID, ScoreRank.S);
		m_RankAExpPayout.Exp = m_ProgressProcessor.GetExpPayout(m_LevelID, ScoreRank.A);
		m_RankBExpPayout.Exp = m_ProgressProcessor.GetExpPayout(m_LevelID, ScoreRank.B);
		m_RankCExpPayout.Exp = m_ProgressProcessor.GetExpPayout(m_LevelID, ScoreRank.C);
		
		if (Shown)
			m_PreviewSource.Play(m_LevelID);
	}
}