using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIResultMenuControlPage : UIResultMenuPage
{
	public override ResultMenuPageType Type => ResultMenuPageType.Control;

	public override bool Valid => true;

	[SerializeField] UISongImage     m_Image;
	[SerializeField] UISongDiscs     m_Discs;
	[SerializeField] UISongLabel     m_Label;
	[SerializeField] UISongRating    m_Rating;
	[SerializeField] UISongRestart   m_Restart;
	[SerializeField] SongPreview     m_Preview;
	[SerializeField] UISongPlatforms m_Platforms;
	[SerializeField] UISongQRCode    m_QR;

	[Inject] ProfileProcessor   m_ProfileProcessor;
	[Inject] ScoreManager       m_ScoreManager;
	[Inject] ScoresProcessor    m_ScoresProcessor;
	[Inject] SongsManager       m_SongsManager;
	[Inject] SongController     m_SongController;
	[Inject] AdsProcessor       m_AdsProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	string m_SongID;

	public override void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		int sourceRank = (int)m_ScoresProcessor.GetRank(m_SongID);
		int targetRank = (int)m_ScoreManager.GetRank();
		
		m_Discs.Rank = (ScoreRank)Mathf.Max(sourceRank, targetRank);
		
		m_Image.Setup(m_SongID);
		m_Label.Setup(m_SongID);
		m_Rating.Setup(m_SongID);
		m_Restart.Setup(m_SongID);
		m_Platforms.Setup(m_SongID);
		
		m_Preview.Stop();
	}

	public override void Play() { }

	public async void Leave()
	{
		await ProcessLeaveAds();
		
		m_Preview.Stop();
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Songs);
		
		m_SongController.Leave("result_leave");
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.ResultMenu);
	}

	public async void Next()
	{
		await ProcessNextAds();
		
		m_Preview.Stop();
		
		m_SongController.Leave("result_next");
		
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.SongNext);
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		
		mainMenu.Select(MainMenuPageType.Songs);
		
		string songID = GetNextSongID();
		
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
		
		if (string.IsNullOrEmpty(songID))
		{
			await m_MenuProcessor.Show(MenuType.MainMenu, true);
			
			await m_MenuProcessor.Hide(MenuType.ResultMenu);
		}
		else
		{
			UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
			
			songMenu.Setup(songID);
			
			await m_MenuProcessor.Show(MenuType.SongMenu);
			
			await m_MenuProcessor.Show(MenuType.MainMenu, true);
			await m_MenuProcessor.Hide(MenuType.ResultMenu, true);
		}
	}

	public void ToggleQR()
	{
		if (m_QR.Shown)
		{
			m_QR.Hide();
		}
		else
		{
			m_QR.Setup(m_SongID);
			m_QR.Show();
		}
	}

	string GetNextSongID()
	{
		string songID = m_SongsManager
			.GetLibrarySongIDs()
			.FirstOrDefault(_SongID => _SongID != m_SongID);
		
		ScoreRank rank = m_ScoresProcessor.GetRank(songID);
		
		if (rank < ScoreRank.Gold)
			return songID;
		
		return m_SongsManager
			.GetLibrarySongIDs()
			.FirstOrDefault(_SongID => _SongID != m_SongID);
	}

	protected override void OnShowStarted()
	{
		m_QR.Hide(true);
	}

	protected override void OnShowFinished()
	{
		m_Preview.Play(m_SongID);
		
		RequestReview();
	}

	protected override void OnHideFinished()
	{
		m_Rating.Execute();
	}

	void RequestReview()
	{
		if (UIReviewMenu.Processed)
			return;
		
		UIReviewMenu reviewMenu = m_MenuProcessor.GetMenu<UIReviewMenu>();
		
		if (reviewMenu == null)
			return;
		
		reviewMenu.Setup(m_ScoreManager.GetRank());
		
		reviewMenu.Process();
	}

	async Task ProcessNextAds()
	{
		if (m_AdsProcessor.CheckUnavailable() || m_ProfileProcessor.HasNoAds())
			return;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_AdsProcessor.Interstitial("result_next");
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	async Task ProcessLeaveAds()
	{
		if (m_AdsProcessor.CheckUnavailable() || m_ProfileProcessor.HasNoAds())
			return;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_AdsProcessor.Interstitial("result_leave");
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}
}