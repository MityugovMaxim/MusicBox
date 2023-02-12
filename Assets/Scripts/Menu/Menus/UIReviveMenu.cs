using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.ReviveMenu)]
public class UIReviveMenu : UIMenu
{
	[SerializeField] TMP_Text      m_Coins;
	[SerializeField] Button        m_ReviveAdsButton;
	[SerializeField] Button        m_ReviveCoinsButton;
	[SerializeField] UIReviveTimer m_Timer;

	[SerializeField, Sound] string m_Sound;

	[Inject] ProfileCoinsParameter m_ProfileCoins;
	[Inject] RevivesManager        m_RevivesManager;
	[Inject] AdsProcessor          m_AdsProcessor;
	[Inject] SongController        m_SongController;
	[Inject] MenuProcessor         m_MenuProcessor;
	[Inject] SoundProcessor        m_SoundProcessor;

	int m_Count;

	public void Setup()
	{
		m_Count = 0;
	}

	public async void ReviveCoins()
	{
		long coins = m_RevivesManager.GetCoins(m_Count);
		
		if (!await m_ProfileCoins.ReduceAsync(coins))
			return;
		
		m_Timer.Pause();
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		SongReviveRequest request = new SongReviveRequest(m_Count);
		
		bool success = await request.SendAsync();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		
		if (success)
		{
			m_Count++;
			
			m_Timer.Complete();
			
			await m_MenuProcessor.Hide(MenuType.ReviveMenu);
			
			await Task.Delay(500);
			
			m_SongController.Revive();
		}
		else
		{
			await m_MenuProcessor.RetryAsync(
				"revive_coins",
				ReviveCoins,
				m_Timer.Resume
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void ReviveAds()
	{
		m_Timer.Pause();
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		bool success = await m_AdsProcessor.Rewarded("revive");
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		
		if (success)
		{
			m_Count++;
			
			m_Timer.Complete();
			
			await m_MenuProcessor.Hide(MenuType.ReviveMenu);
			
			await Task.Delay(500);
			
			m_SongController.Revive();
		}
		else
		{
			await m_MenuProcessor.RetryAsync(
				"song_revive_ads",
				ReviveAds,
				m_Timer.Resume
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

	protected override async void OnShowStarted()
	{
		base.OnShowStarted();
		
		m_SoundProcessor.Play(m_Sound);
		
		bool success = await m_Timer.ProcessAsync();
		
		if (success)
			return;
		
		m_SongController.Finish();
	}
}
