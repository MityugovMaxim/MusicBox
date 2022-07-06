using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIResultMenuRewardPage : UIResultMenuPage
{
	public override ResultMenuPageType Type => ResultMenuPageType.Reward;

	public override bool Valid => true;

	[SerializeField] UISongScoreList m_ScoreList;
	[SerializeField] Button          m_DoubleButton;
	[SerializeField] UIGroup         m_ContinueGroup;
	[SerializeField] UIGroup         m_LoaderGroup;

	[Inject] ScoreManager    m_ScoreManager;
	[Inject] ScoresProcessor m_ScoresProcessor;
	[Inject] AdsProcessor    m_AdsProcessor;
	[Inject] MenuProcessor   m_MenuProcessor;

	string    m_SongID;
	bool      m_Double;
	ScoreRank m_SourceRank;
	ScoreRank m_TargetRank;

	protected override void Awake()
	{
		base.Awake();
		
		m_DoubleButton.onClick.AddListener(Double);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_DoubleButton.onClick.RemoveListener(Double);
	}

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
		await m_ScoreList.PlayAsync();
		
		await m_ContinueGroup.ShowAsync();
	}

	public async void Double()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		bool success = await m_AdsProcessor.Rewarded();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		
		if (success)
		{
			await m_MenuProcessor.Hide(MenuType.ReviveMenu);
			
			m_Double = true;
			
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
		
		m_ContinueGroup.Hide();
		m_LoaderGroup.Show();
		
		SongFinishRequest request = new SongFinishRequest(
			m_SongID,
			m_ScoreManager.GetScore(),
			m_ScoreManager.GetAccuracy()
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

	void Next()
	{
		UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
		
		resultMenu.Next();
	}
}
