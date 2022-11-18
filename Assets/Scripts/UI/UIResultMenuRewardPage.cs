using UnityEngine;
using Zenject;

public class UIResultMenuRewardPage : UIResultMenuPage
{
	public override ResultMenuPageType Type => ResultMenuPageType.Reward;

	public override bool Valid => true;

	[SerializeField] UISongScoreList m_ScoreList;
	[SerializeField] UIGroup         m_ContinueGroup;
	[SerializeField] UIGroup         m_LoaderGroup;

	[Inject] ScoreController m_ScoreController;
	[Inject] AdsProcessor    m_AdsProcessor;
	[Inject] MenuProcessor   m_MenuProcessor;

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
		
		bool success = await m_AdsProcessor.Rewarded("double");
		
		if (success)
		{
			await m_MenuProcessor.Hide(MenuType.ReviveMenu);
			
			m_Double = true;
			
			await m_ScoreList.DoubleAsync();
			
			Continue();
		}
		else
		{
			await m_MenuProcessor.RetryAsync(
				"double",
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
			m_ScoreController.GetScore(),
			m_ScoreController.GetAccuracy(),
			m_Double
		);
		
		bool success = await request.SendAsync();
		
		if (success)
		{
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
			
			Next();
		}
		else
		{
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
			
			await m_MenuProcessor.RetryAsync(
				"song_finish",
				Continue,
				Next
			);
		}
	}

	void Next()
	{
		UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
		
		resultMenu.Next();
	}
}
