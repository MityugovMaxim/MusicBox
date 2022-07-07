using UnityEngine;
using Zenject;

[Menu(MenuType.PauseMenu)]
public class UIPauseMenu : UIMenu
{
	[SerializeField] UISongImage  m_Image;
	[SerializeField] UISongLabel  m_Label;
	[SerializeField] UISongQRCode m_QR;

	[Inject] ProfileProcessor m_ProfileProcessor;
	[Inject] SongController   m_SongController;
	[Inject] AdsProcessor     m_AdsProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
	}

	public async void Restart()
	{
		if (m_AdsProcessor.CheckAvailable() && !m_ProfileProcessor.HasNoAds())
		{
			await m_MenuProcessor.Show(MenuType.BlockMenu, true);
			
			await m_AdsProcessor.Interstitial();
			
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		}
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		m_SongController.Restart();
		
		await m_MenuProcessor.Hide(MenuType.PauseMenu);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void Leave()
	{
		if (m_AdsProcessor.CheckAvailable() && !m_ProfileProcessor.HasNoAds())
		{
			await m_MenuProcessor.Show(MenuType.BlockMenu, true);
			
			await m_AdsProcessor.Interstitial();
			
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		}
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Songs);
		
		UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
		if (songMenu != null)
			songMenu.Setup(m_SongID);
		
		m_SongController.Leave();
		
		await m_MenuProcessor.Show(MenuType.SongMenu);
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
	}

	public async void Resume()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		m_SongController.Resume();
		
		await m_MenuProcessor.Hide(MenuType.PauseMenu);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void Latency()
	{
		await m_MenuProcessor.Show(MenuType.LatencyMenu);
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

	protected override void OnShowStarted()
	{
		m_Image.Setup(m_SongID);
		m_Label.Setup(m_SongID);
		
		m_QR.Hide(true);
	}

	protected override bool OnEscape()
	{
		Leave();
		
		return true;
	}
}
