using System.Linq;
using UnityEngine;
using Zenject;

public class UIResultMenuControlPage : UIResultMenuPage
{
	public override ResultMenuPageType Type => ResultMenuPageType.Control;

	public override bool Valid => true;

	[SerializeField] UISongImage  m_Image;
	[SerializeField] UISongDiscs  m_Discs;
	[SerializeField] UISongLabel  m_Label;
	[SerializeField] SongPreview  m_Preview;
	[SerializeField] UISongQRCode m_QR;

	[Inject] ScoresManager      m_ScoresManager;
	[Inject] SongsManager       m_SongsManager;
	[Inject] SongController     m_SongController;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	string m_SongID;

	public override void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Discs.SongID = m_SongID;
		
		m_Image.SongID  = m_SongID;
		m_Label.SongID  = m_SongID;
		
		m_Preview.Stop();
	}

	public override void Play() { }

	public async void Leave()
	{
		m_Preview.Stop();
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Songs);
		
		m_SongController.Complete();
		
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.SongLeave);
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.ResultMenu);
	}

	public async void Next()
	{
		m_Preview.Stop();
		
		m_SongController.Complete();
		
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
		
		ScoreRank rank = m_ScoresManager.GetRank(songID);
		
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

	void RequestReview()
	{
		if (UIReviewMenu.Processed)
			return;
		
		UIReviewMenu reviewMenu = m_MenuProcessor.GetMenu<UIReviewMenu>();
		
		if (reviewMenu == null)
			return;
		
		reviewMenu.Setup(m_ScoresManager.GetRank(m_SongID));
		
		reviewMenu.Process();
	}
}
