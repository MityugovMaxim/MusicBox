using System;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIResultMenu : UIMenu, IInitializable, IDisposable
{
	const int RESTART_ADS_COUNT = 2;
	const int LEAVE_ADS_COUNT   = 3;
	const int NEXT_ADS_COUNT    = 2;

	[SerializeField] UILevelPreviewBackground m_Background;
	[SerializeField] UILevelPreviewThumbnail  m_Thumbnail;
	[SerializeField] UILevelPreviewLabel      m_Label;
	[SerializeField] UIScore                  m_Score;
	[SerializeField] UILevelLike              m_LikeButton;
	[SerializeField] UILevelModeButton        m_RestartButton;

	SignalBus       m_SignalBus;
	UIMainMenu      m_MainMenu;
	UILevelMenu     m_LevelMenu;
	LevelProcessor  m_LevelProcessor;
	ScoreProcessor  m_ScoreProcessor;
	SocialProcessor m_SocialProcessor;
	AdsProcessor    m_AdsProcessor;

	string    m_LevelID;
	ScoreData m_ScoreData;

	int m_RestartAdsCount;
	int m_LeaveAdsCount;
	int m_NextAdsCount;

	[Inject]
	public void Construct(
		SignalBus       _SignalBus,
		UIMainMenu      _MainMenu,
		UILevelMenu     _LevelMenu,
		LevelProcessor  _LevelProcessor,
		ScoreProcessor  _ScoreProcessor,
		SocialProcessor _SocialProcessor,
		AdsProcessor    _AdsProcessor
	)
	{
		m_SignalBus       = _SignalBus;
		m_MainMenu        = _MainMenu;
		m_LevelMenu       = _LevelMenu;
		m_LevelProcessor  = _LevelProcessor;
		m_ScoreProcessor  = _ScoreProcessor;
		m_SocialProcessor = _SocialProcessor;
		m_AdsProcessor    = _AdsProcessor;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelFinishSignal>(RegisterLevelFinish);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelFinishSignal>(RegisterLevelFinish);
	}

	void RegisterLevelStart(LevelStartSignal _Signal)
	{
		m_LevelID = _Signal.LevelID;
		
		Hide(true);
	}

	void RegisterLevelFinish(LevelFinishSignal _Signal)
	{
		m_LevelID = _Signal.LevelID;
		
		m_ScoreData = m_ScoreProcessor.ScoreData;
		
		m_ScoreProcessor.SaveLastScore(m_LevelID, m_ScoreData);
		m_ScoreProcessor.SaveBestScore(m_LevelID, m_ScoreData);
		
		m_Background.Setup(m_LevelID, true);
		m_Thumbnail.Setup(m_LevelID);
		m_Label.Setup(m_LevelID);
		m_Score.Setup(m_LevelID);
		m_LikeButton.Setup(m_LevelID);
		m_RestartButton.Setup(m_LevelID);
		
		string leaderboardID = m_LevelProcessor.GetLeaderboardID(m_LevelID);
		long   score         = m_ScoreProcessor.GetBestScore(m_LevelID);
		m_SocialProcessor.ReportScore(leaderboardID, score);
		
		string    achievementID = m_LevelProcessor.GetAchievementID(m_LevelID);
		ScoreRank rank          = m_ScoreProcessor.GetBestRank(m_LevelID);
		if (rank == ScoreRank.S)
			m_SocialProcessor.CompleteAchievement(achievementID);
		
		Show();
	}

	[Preserve]
	public void Restart()
	{
		void RestartInternal()
		{
			if (m_LevelProcessor == null)
			{
				Debug.LogError("[UIResultMenu] Restart level failed. Level provider is null.", gameObject);
				return;
			}
			
			m_LevelProcessor.Restart();
			
			CloseAction = m_LevelProcessor.Play;
			
			Hide();
		}
		
		LevelMode levelMode = m_LevelProcessor.GetLevelMode(m_LevelID);
		
		if (levelMode == LevelMode.Ads)
		{
			m_AdsProcessor.ShowRewarded(RestartInternal);
		}
		else
		{
			m_RestartAdsCount++;
			
			if (m_RestartAdsCount >= RESTART_ADS_COUNT)
			{
				m_RestartAdsCount = 0;
				
				m_AdsProcessor.ShowInterstitial(RestartInternal);
			}
			else
			{
				RestartInternal();
			}
		}
	}

	[Preserve]
	public void Leave()
	{
		void LeaveInternal()
		{
			if (m_LevelProcessor == null)
			{
				Debug.LogError("[UIResultMenu] Leave level failed. Level provider is null.", gameObject);
				return;
			}
			
			m_LevelProcessor.Remove();
			
			if (m_MainMenu != null)
				m_MainMenu.Show();
		}
		
		m_LeaveAdsCount++;
		
		if (m_LeaveAdsCount >= LEAVE_ADS_COUNT)
		{
			m_LeaveAdsCount = 0;
			
			m_AdsProcessor.ShowInterstitial(LeaveInternal);
		}
		else
		{
			LeaveInternal();
		}
	}

	[Preserve]
	public void Next()
	{
		void NextInternal()
		{
			if (m_LevelProcessor == null)
			{
				Debug.LogError("[UIResultMenu] Leave level failed. Level provider is null.", gameObject);
				return;
			}
			
			m_LevelProcessor.Remove();
			
			if (m_MainMenu != null)
				m_MainMenu.Show();
			
			if (m_LevelMenu != null)
			{
				string levelID = m_LevelProcessor.GetNextLevelID(m_LevelID);
				m_LevelMenu.Setup(levelID);
				m_LevelMenu.Show();
			}
		}
		
		m_NextAdsCount++;
		
		if (m_NextAdsCount >= NEXT_ADS_COUNT)
		{
			m_NextAdsCount = 0;
			
			m_AdsProcessor.ShowInterstitial(NextInternal);
		}
		else
		{
			NextInternal();
		}
	}

	[Preserve]
	public void Leaderboard()
	{
		string leaderboardID = m_LevelProcessor.GetLeaderboardID(m_LevelID);
		m_SocialProcessor.ShowLeaderboard(leaderboardID);
	}

	[Preserve]
	public void Achievements()
	{
		m_SocialProcessor.ShowAchievements();
	}

	protected override void OnShowStarted()
	{
		if (m_Score != null)
			m_Score.Restore();
	}

	protected override void OnShowFinished()
	{
		if (m_Score != null)
			m_Score.Play();
		
		if (m_LevelProcessor != null)
			m_LevelProcessor.Pause();
	}

	protected override void OnHideStarted()
	{
		Debug.LogError("---> HIDE RESULT");
		
		if (m_LikeButton != null)
			m_LikeButton.Execute();
	}

	protected override void OnHideFinished()
	{
		if (m_Score != null)
			m_Score.Restore();
	}
}