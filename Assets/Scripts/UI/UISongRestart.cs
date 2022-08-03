using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UISongRestart : UIEntity
{
	[SerializeField] UIGroup     m_ControlGroup;
	[SerializeField] UIGroup     m_LoaderGroup;
	[SerializeField] UIUnitLabel m_Coins;
	[SerializeField] GameObject  m_Free;
	[SerializeField] GameObject  m_Ads;
	[SerializeField] GameObject  m_Paid;
	[SerializeField] SongPreview m_Preview;

	[Inject] SongsManager     m_SongsManager;
	[Inject] SongsProcessor   m_SongsProcessor;
	[Inject] ProfileProcessor m_ProfileProcessor;
	[Inject] AdsProcessor     m_AdsProcessor;
	[Inject] SongController   m_SongController;
	[Inject] MenuProcessor    m_MenuProcessor;

	string   m_SongID;
	SongMode m_Mode;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		m_Mode   = SongMode.Free;
		
		m_ControlGroup.Show(true);
		m_LoaderGroup.Hide(true);
		
		SetFreeActive(false);
		SetAdsActive(false);
		SetPaidActive(false);
		
		switch (m_Mode)
		{
			case SongMode.Free:
				SetFreeActive(true);
				break;
			case SongMode.Ads:
				SetAdsActive(true);
				break;
			case SongMode.Paid:
				SetPaidActive(true);
				break;
			default:
				SetFreeActive(true);
				break;
		}
	}

	public void Restart()
	{
		switch (m_Mode)
		{
			case SongMode.Free:
				RestartFree();
				break;
			case SongMode.Ads:
				RestartAds();
				break;
			case SongMode.Paid:
				RestartPaid();
				break;
			default:
				RestartAds();
				break;
		}
	}

	async void RestartFree()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		if (m_AdsProcessor.CheckAvailable() && !m_ProfileProcessor.HasNoAds())
		{
			await m_ControlGroup.HideAsync();
			await m_LoaderGroup.ShowAsync();
			
			await m_AdsProcessor.Interstitial("song_restart");
			
			await m_LoaderGroup.HideAsync();
			await m_ControlGroup.ShowAsync();
		}
		
		StopPreview();
		
		m_SongController.Restart();
		
		await Task.WhenAll(
			m_MenuProcessor.Hide(MenuType.PauseMenu),
			m_MenuProcessor.Hide(MenuType.ReviveMenu),
			m_MenuProcessor.Hide(MenuType.ResultMenu)
		);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	async void RestartAds()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_ControlGroup.HideAsync();
		await m_LoaderGroup.ShowAsync();
		
		bool success = await m_AdsProcessor.Rewarded(null);
		
		await m_LoaderGroup.HideAsync();
		await m_ControlGroup.ShowAsync();
		
		if (success)
		{
			StopPreview();
			
			m_SongController.Restart();
			
			await Task.WhenAll(
				m_MenuProcessor.Hide(MenuType.PauseMenu),
				m_MenuProcessor.Hide(MenuType.ReviveMenu),
				m_MenuProcessor.Hide(MenuType.ResultMenu)
			);
		}
		else
		{
			await m_MenuProcessor.RetryLocalizedAsync(
				"song_restart_ads",
				"song_restart_button",
				"SONG_RESTART_ADS_ERROR_TITLE",
				"COMMON_ERROR_MESSAGE",
				RestartAds,
				() => { }
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	async void RestartPaid()
	{
		if (m_SongsManager.IsSongAvailable(m_SongID))
		{
			RestartFree();
			return;
		}
		
		string songID = m_SongID;
		
		long coins = m_SongsProcessor.GetPrice(songID);
		
		if (!await m_ProfileProcessor.CheckCoins(coins))
			return;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_ControlGroup.HideAsync();
		await m_LoaderGroup.ShowAsync();
		
		SongUnlockRequest request = new SongUnlockRequest(songID);
		
		bool success = await request.SendAsync();
		
		await m_LoaderGroup.HideAsync();
		await m_ControlGroup.ShowAsync();
		
		if (success)
		{
			StopPreview();
			
			m_SongController.Restart();
			
			await Task.WhenAll(
				m_MenuProcessor.Hide(MenuType.PauseMenu),
				m_MenuProcessor.Hide(MenuType.ReviveMenu),
				m_MenuProcessor.Hide(MenuType.ResultMenu)
			);
		}
		else
		{
			await m_MenuProcessor.RetryLocalizedAsync(
				"song_restart_paid",
				"song_restart_button",
				"SONG_RESTART_PAID_ERROR_TITLE",
				"COMMON_ERROR_MESSAGE",
				RestartPaid,
				() => { }
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	void StopPreview()
	{
		if (m_Preview != null)
			m_Preview.Stop();
	}

	void SetFreeActive(bool _Value)
	{
		if (m_Free != null)
			m_Free.SetActive(_Value);
	}

	void SetAdsActive(bool _Value)
	{
		if (m_Ads != null)
			m_Ads.SetActive(_Value);
	}

	void SetPaidActive(bool _Value)
	{
		if (m_Paid != null)
			m_Paid.SetActive(_Value);
		
		if (m_Coins != null)
			m_Coins.Value = m_SongsProcessor.GetPrice(m_SongID);
	}
}
