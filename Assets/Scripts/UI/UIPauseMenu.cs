using UnityEngine;
using Zenject;

public class UIPauseMenu : UIMenu
{
	const int RESTART_ADS_COUNT = 2;
	const int LEAVE_ADS_COUNT   = 3;

	[SerializeField] UILevelPreviewThumbnail m_Thumbnail;

	MenuProcessor  m_MenuProcessor;
	LevelProcessor m_LevelProcessor;
	AdsProcessor   m_AdsProcessor;

	int m_RestartAdsCount;
	int m_LeaveAdsCount;

	[Inject]
	public void Construct(
		MenuProcessor  _MenuProcessor,
		LevelProcessor _LevelProcessor,
		AdsProcessor   _AdsProcessor
	)
	{
		m_MenuProcessor  = _MenuProcessor;
		m_LevelProcessor = _LevelProcessor;
		m_AdsProcessor   = _AdsProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_Thumbnail.Setup(_LevelID);
	}

	public void Pause()
	{
		if (m_LevelProcessor != null)
			m_LevelProcessor.Pause();
		
		Show();
	}

	public void Resume()
	{
		if (m_LevelProcessor != null)
			m_LevelProcessor.Play();
		
		Hide();
	}

	public void Restart()
	{
		void RestartInternal()
		{
			if (m_LevelProcessor == null)
			{
				Debug.LogError("[UIPauseMenu] Restart level failed. Level provider is null.", gameObject);
				return;
			}
			
			m_LevelProcessor.Restart();
			
			CloseAction = m_LevelProcessor.Play;
			
			Hide();
		}
		
		m_RestartAdsCount++;
		
		if (m_RestartAdsCount >= RESTART_ADS_COUNT)
		{
			m_RestartAdsCount = 0;
			
			m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			m_AdsProcessor.ShowInterstitialAsync(
				this,
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

	public void Leave()
	{
		void LeaveInternal()
		{
			if (m_LevelProcessor == null)
			{
				Debug.LogError("[UIPauseMenu] Leave level failed. Level provider is null.", gameObject);
				return;
			}
			
			m_LevelProcessor.Remove();
			
			m_MenuProcessor.Show(MenuType.MainMenu)
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
}
