using UnityEngine;
using Zenject;

public class UIResultMenuRewardPage : UIResultMenuPage
{
	public override ResultMenuPageType Type => ResultMenuPageType.Reward;

	public override bool Valid => true;

	[SerializeField] UISongScoreList m_ScoreList;
	[SerializeField] UIGroup         m_ContinueGroup;
	[SerializeField] UIGroup         m_LoaderGroup;

	[Inject] ScoreManager     m_ScoreManager;
	[Inject] ScoresProcessor  m_ScoresProcessor;
	[Inject] AdsProcessor     m_AdsProcessor;
	[Inject] ProfileProcessor m_ProfileProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;

	string    m_SongID;
	bool      m_Double;
	ScoreRank m_SourceRank;
	ScoreRank m_TargetRank;

	public override void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Double = false;
		
		m_ScoreList.Setup(m_SongID);
		
		m_ContinueGroup.Hide(true);
		m_LoaderGroup.Hide(true);
	}

	public override async void Play()
	{
		await m_ContinueGroup.ShowAsync();
		
		await m_ScoreList.PlayAsync();
	}

	public async void Double()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_ContinueGroup.HideAsync();
		await m_LoaderGroup.ShowAsync();
		
		bool success = await m_AdsProcessor.Rewarded();
		
		if (success)
		{
			await m_MenuProcessor.Hide(MenuType.ReviveMenu);
			
			m_Double = true;
			
			await m_ScoreList.DoubleAsync();
			
			Continue();
		}
		else
		{
			await m_MenuProcessor.RetryLocalizedAsync(
				"song_revive_ads",
				"revive_menu",
				"SONG_REVIVE_ERROR_TITLE",
				"SONG_REVIVE_ERROR_MESSAGE",
				Double,
				Continue
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void Continue()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_ContinueGroup.HideAsync();
		await m_LoaderGroup.ShowAsync();
		
		SongFinishRequest request = new SongFinishRequest(
			m_SongID,
			m_ScoreManager.GetScore(),
			m_ScoreManager.GetAccuracy(),
			m_Double
		);
		
		bool success = await request.SendAsync();
		
		if (success)
		{
			await m_ScoresProcessor.Load();
			
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
			
			Next();
		}
		else
		{
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
			
			await m_MenuProcessor.RetryLocalizedAsync(
				"song_finish",
				"result_menu",
				"SONG_FINISH_ERROR_TITLE",
				"SONG_FINISH_ERROR_MESSAGE",
				Continue,
				Next
			);
		}
	}

	async void Next()
	{
		if (m_AdsProcessor.CheckAvailable() && !m_ProfileProcessor.HasNoAds())
		{
			await m_MenuProcessor.Show(MenuType.BlockMenu, true);
			
			await m_ContinueGroup.HideAsync();
			await m_LoaderGroup.ShowAsync();
			
			await m_AdsProcessor.Interstitial();
			
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		}
		
		UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
		
		resultMenu.Next();
	}
}
