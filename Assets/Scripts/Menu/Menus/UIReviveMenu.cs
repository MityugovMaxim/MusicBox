using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.ReviveMenu)]
public class UIReviveMenu : UIMenu
{
	[SerializeField] UISongImage m_Image;
	[SerializeField] UIUnitLabel m_Coins;

	[SerializeField, Sound] string m_Sound;

	[Inject] CoinsParameter   m_CoinsParameter;
	[Inject] AdsProcessor     m_AdsProcessor;
	[Inject] SongController   m_SongController;
	[Inject] RevivesProcessor m_RevivesProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;
	[Inject] SoundProcessor   m_SoundProcessor;

	string m_SongID;
	int    m_Count;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		m_Count  = 0;
	}

	public async void ReviveCoins()
	{
		long coins = m_RevivesProcessor.GetCoins(m_Count);
		
		if (!await m_CoinsParameter.Remove(coins))
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
			
			m_SongController.Revive();
		}
		else
		{
			await m_MenuProcessor.RetryAsync(
				"song_revive_coins",
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
		
		bool success = await m_AdsProcessor.Rewarded("revive");
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		
		if (success)
		{
			m_Count++;
			
			await m_MenuProcessor.Hide(MenuType.ReviveMenu);
			
			await Task.Delay(500);
			
			m_SongController.Revive();
		}
		else
		{
			await m_MenuProcessor.RetryAsync(
				"song_revive_ads",
				ReviveAds,
				() => { }
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void Restart()
	{
		m_SongController.Restart();
		
		await m_MenuProcessor.Show(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
		await m_MenuProcessor.Hide(MenuType.ReviveMenu, true);
	}

	public async void Leave()
	{
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Songs);
		
		UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
		if (songMenu != null)
			songMenu.Setup(m_SongID);
		
		m_SongController.Complete();
		
		await m_MenuProcessor.Show(MenuType.SongMenu);
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.ReviveMenu, true);
		await m_MenuProcessor.Hide(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
	}

	protected override void OnShowStarted()
	{
		m_SoundProcessor.Play(m_Sound);
		
		m_Image.SongID = m_SongID;
		
		m_Coins.Value = m_RevivesProcessor.GetCoins(m_Count);
	}
}
