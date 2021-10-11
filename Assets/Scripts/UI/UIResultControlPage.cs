using UnityEngine;
using UnityEngine.iOS;
using Zenject;

public class UIResultControlPage : UIResultMenuPage
{
	// TODO: Move to remote config
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

	MenuProcessor   m_MenuProcessor;
	LevelProcessor  m_LevelProcessor;
	AdsProcessor    m_AdsProcessor;
	HapticProcessor m_HapticProcessor;

	int m_LeaveAdsCount;
	int m_NextAdsCount;
	int m_RestartAdsCount;
	int m_RateUsCount;

	string m_LevelID;

	[Inject]
	public void Construct(
		MenuProcessor   _MenuProcessor,
		LevelProcessor  _LevelProcessor,
		AdsProcessor    _AdsProcessor,
		HapticProcessor _HapticProcessor
	)
	{
		m_MenuProcessor   = _MenuProcessor;
		m_LevelProcessor  = _LevelProcessor;
		m_AdsProcessor    = _AdsProcessor;
		m_HapticProcessor = _HapticProcessor;
	}

	public override void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		m_Thumbnail.Setup(m_LevelID);
		m_Label.Setup(m_LevelID);
		m_LikeButton.Setup(m_LevelID);
		m_RestartButton.Setup(m_LevelID);
	}

	public async void Leave()
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		m_LeaveAdsCount++;
		
		if (m_LeaveAdsCount >= LEAVE_ADS_COUNT)
		{
			m_LeaveAdsCount = 0;
			
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			await m_AdsProcessor.ShowInterstitialAsync(this);
			
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
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		m_NextAdsCount++;
		
		if (m_NextAdsCount >= NEXT_ADS_COUNT)
		{
			m_NextAdsCount = 0;
			
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			await m_AdsProcessor.ShowInterstitialAsync(this);
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		}
		
		m_LevelProcessor.Remove();
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Levels);
		
		UILevelMenu levelMenu = m_MenuProcessor.GetMenu<UILevelMenu>();
		if (levelMenu != null)
			levelMenu.Setup(m_LevelProcessor.GetNextLevelID(m_LevelID));
		
		await m_MenuProcessor.Show(MenuType.LevelMenu);
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.ResultMenu, true);
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
	}

	public async void Restart()
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		LevelMode levelMode = m_LevelProcessor.GetLevelMode(m_LevelID);
		
		if (levelMode == LevelMode.Ads)
		{
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			await m_AdsProcessor.ShowRewardedAsync(this);
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		}
		else
		{
			m_RestartAdsCount++;
			
			if (m_RestartAdsCount >= RESTART_ADS_COUNT)
			{
				m_RestartAdsCount = 0;
				
				await m_MenuProcessor.Show(MenuType.ProcessingMenu);
				
				await m_AdsProcessor.ShowInterstitialAsync(this);
				
				await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
			}
		}
		
		m_LevelProcessor.Restart();
		
		await m_MenuProcessor.Show(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
		await m_MenuProcessor.Hide(MenuType.ResultMenu);
		
		m_LevelProcessor.Play();
	}

	protected override void OnShowFinished()
	{
		if (m_RateUsCount >= RATE_US_COUNT)
		{
			m_RateUsCount = 0;
			
			Device.RequestStoreReview();
		}
	}

	protected override void OnHideFinished()
	{
		m_LikeButton.Execute();
	}
}