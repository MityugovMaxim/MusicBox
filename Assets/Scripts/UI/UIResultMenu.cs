using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.Scripting;
using Zenject;
using Debug = UnityEngine.Debug;

public class UIResultMenu : UIMenu
{
	const int RESTART_ADS_COUNT = 2;
	const int LEAVE_ADS_COUNT   = 3;
	const int NEXT_ADS_COUNT    = 2;
	const int RATE_US_COUNT     = 2;

	[SerializeField] UILevelPreviewBackground m_Background;
	[SerializeField] UILevelPreviewThumbnail  m_Thumbnail;
	[SerializeField] UILevelPreviewLabel      m_Label;
	[SerializeField] UIScore                  m_Score;
	[SerializeField] UILevelLike              m_LikeButton;
	[SerializeField] UILevelModeButton        m_RestartButton;

	MenuProcessor   m_MenuProcessor;
	LevelProcessor  m_LevelProcessor;
	ScoreProcessor  m_ScoreProcessor;
	SocialProcessor m_SocialProcessor;
	AdsProcessor    m_AdsProcessor;
	HapticProcessor m_HapticProcessor;

	string    m_LevelID;
	ScoreData m_ScoreData;

	int m_RestartAdsCount;
	int m_LeaveAdsCount;
	int m_NextAdsCount;
	int m_RateUsCount;

	[Inject]
	public void Construct(
		MenuProcessor   _MenuProcessor,
		LevelProcessor  _LevelProcessor,
		ScoreProcessor  _ScoreProcessor,
		SocialProcessor _SocialProcessor,
		AdsProcessor    _AdsProcessor,
		HapticProcessor _HapticProcessor
	)
	{
		m_MenuProcessor   = _MenuProcessor;
		m_LevelProcessor  = _LevelProcessor;
		m_ScoreProcessor  = _ScoreProcessor;
		m_SocialProcessor = _SocialProcessor;
		m_AdsProcessor    = _AdsProcessor;
		m_HapticProcessor = _HapticProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
		
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
	}

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
			
			m_MenuProcessor.Hide(MenuType.NotificationMenu);
			
			Hide();
		}
		
		LevelMode levelMode = m_LevelProcessor.GetLevelMode(m_LevelID);
		
		if (levelMode == LevelMode.Ads)
		{
			m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			m_AdsProcessor.ShowRewardedAsync(
				this,
				15,
				() =>
				{
					m_MenuProcessor.Hide(MenuType.ProcessingMenu, true);
					
					RestartInternal();
				},
				() => m_MenuProcessor.Hide(MenuType.ProcessingMenu, true),
				() => m_MenuProcessor.Hide(MenuType.ProcessingMenu)
			);
		}
		else
		{
			m_RestartAdsCount++;
			
			if (m_RestartAdsCount >= RESTART_ADS_COUNT)
			{
				m_RestartAdsCount = 0;
				
				m_MenuProcessor.Show(MenuType.ProcessingMenu);
				
				m_AdsProcessor.ShowInterstitialAsync(
					this,
					15,
					() =>
					{
						m_MenuProcessor.Hide(MenuType.ProcessingMenu, true);
						
						RestartInternal();
					}
				);
			}
			else
			{
				RestartInternal();
			}
		}
	}

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
			
			m_MenuProcessor.Show(MenuType.MainMenu)
				.ThenHide(MenuType.ResultMenu, true)
				.ThenHide(MenuType.GameMenu, true)
				.ThenHide(MenuType.PauseMenu, true);
		}
		
		m_LeaveAdsCount++;
		
		if (m_LeaveAdsCount >= LEAVE_ADS_COUNT)
		{
			m_LeaveAdsCount = 0;
			
			m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			m_AdsProcessor.ShowInterstitialAsync(
				this,
				15,
				() =>
				{
					m_MenuProcessor.Hide(MenuType.ProcessingMenu, true);
					
					LeaveInternal();
				}
			);
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
			
			UILevelMenu levelMenu = m_MenuProcessor.GetMenu<UILevelMenu>(MenuType.LevelMenu);
			if (levelMenu != null)
				levelMenu.Setup(m_LevelProcessor.GetNextLevelID(m_LevelID));
			
			m_MenuProcessor.Show(MenuType.MainMenu)
				.ThenHide(MenuType.ResultMenu, true)
				.ThenHide(MenuType.GameMenu, true)
				.ThenHide(MenuType.PauseMenu, true);
			m_MenuProcessor.Show(MenuType.LevelMenu);
		}
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		m_NextAdsCount++;
		
		if (m_NextAdsCount >= NEXT_ADS_COUNT)
		{
			m_NextAdsCount = 0;
			
			m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			m_AdsProcessor.ShowInterstitialAsync(
				this,
				15,
				() =>
				{
					m_MenuProcessor.Hide(MenuType.ProcessingMenu, true);
					
					NextInternal();
				}
			);
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
		void ScoreFinished()
		{
			m_MenuProcessor.Show(MenuType.NotificationMenu);
			
			if (m_RateUsCount >= RATE_US_COUNT)
			{
				m_RateUsCount = 0;
				
				Device.RequestStoreReview();
			}
		}
		
		if (m_Score != null)
			m_Score.Play(ScoreFinished);
		else
			ScoreFinished();
		
		if (m_LevelProcessor != null)
			m_LevelProcessor.Pause();
	}

	protected override void OnHideStarted()
	{
		if (m_LikeButton != null)
			m_LikeButton.Execute();
	}

	protected override void OnHideFinished()
	{
		if (m_Score != null)
			m_Score.Restore();
	}
}