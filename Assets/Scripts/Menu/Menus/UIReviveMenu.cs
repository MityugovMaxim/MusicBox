using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.ReviveMenu)]
public class UIReviveMenu : UIMenu
{
	[SerializeField] UISongImage m_Image;
	[SerializeField] UISongMode  m_Restart;
	[SerializeField] UIUnitLabel m_Coins;

	[SerializeField, Sound] string m_Sound;

	[Inject] AdsProcessor     m_AdsProcessor;
	[Inject] ConfigProcessor  m_ConfigProcessor;
	[Inject] ProfileProcessor m_ProfileProcessor;
	[Inject] SongsProcessor   m_SongsProcessor;
	[Inject] SongController   m_SongController;
	[Inject] RevivesProcessor m_RevivesProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;
	[Inject] SoundProcessor   m_SoundProcessor;

	string m_SongID;
	int    m_Count;
	int    m_RestartAdsCount;
	int    m_LeaveAdsCount;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		m_Count  = 0;
		
		ProcessCoins();
	}

	public async void ReviveCoins()
	{
		long coins = m_RevivesProcessor.GetCoins(m_Count);
		
		if (!await m_ProfileProcessor.CheckCoins(coins))
			return;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		SongReviveRequest request = new SongReviveRequest(m_Count);
		
		bool success = await request.SendAsync();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		
		if (success)
		{
			m_Count++;
			
			await m_MenuProcessor.Hide(MenuType.ReviveMenu);
			
			await Task.Delay(500);
			
			m_SongController.Revive(false);
		}
		else
		{
			await m_MenuProcessor.RetryLocalizedAsync(
				"song_revive_coins",
				"SONG_REVIVE_ERROR_TITLE",
				"SONG_REVIVE_ERROR_MESSAGE",
				ReviveCoins,
				() => { }
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void ReviveAds()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		bool success = await m_AdsProcessor.Rewarded();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		
		if (success)
		{
			m_Count++;
			
			await m_MenuProcessor.Hide(MenuType.ReviveMenu);
			
			await Task.Delay(500);
			
			m_SongController.Revive(true);
		}
		else
		{
			await m_MenuProcessor.RetryLocalizedAsync(
				"song_revive_ads",
				"SONG_REVIVE_ERROR_TITLE",
				"SONG_REVIVE_ERROR_MESSAGE",
				ReviveAds,
				() => { }
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	protected override void OnShowStarted()
	{
		m_SoundProcessor.Play(m_Sound);
		m_Image.Setup(m_SongID);
		m_Restart.Setup(m_SongID);
		
		ProcessCoins();
	}

	public async void Restart()
	{
		if (!await ProcessRestartAds())
			return;
		
		m_SongController.Restart();
		
		await m_MenuProcessor.Show(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
		await m_MenuProcessor.Hide(MenuType.ReviveMenu, true);
	}

	public async void Leave()
	{
		await ProcessLeaveAds();
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Songs);
		
		UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
		if (songMenu != null)
			songMenu.Setup(m_SongID);
		
		m_SongController.Leave();
		
		await m_MenuProcessor.Show(MenuType.SongMenu);
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.ReviveMenu, true);
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
	}

	async Task<bool> ProcessRestartAds()
	{
		if (m_ProfileProcessor.HasNoAds())
			return true;
		
		SongMode songMode = m_SongsProcessor.GetMode(m_SongID);
		
		if (songMode == SongMode.Ads)
		{
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			bool success = await m_AdsProcessor.Rewarded();
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
			
			return success;
		}
		else
		{
			m_RestartAdsCount++;
			
			if (m_RestartAdsCount < m_ConfigProcessor.SongRestartAdsCount)
				return true;
			
			m_RestartAdsCount = 0;
			
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			await m_AdsProcessor.Interstitial();
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
			
			return true;
		}
	}

	async Task ProcessLeaveAds()
	{
		if (m_ProfileProcessor.HasNoAds())
			return;
		
		m_LeaveAdsCount++;
		
		if (m_LeaveAdsCount < m_ConfigProcessor.SongLeaveAdsCount)
			return;
		
		m_LeaveAdsCount = 0;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_AdsProcessor.Interstitial();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	void ProcessCoins()
	{
		m_Coins.Value = m_RevivesProcessor.GetCoins(m_Count);
	}
}
