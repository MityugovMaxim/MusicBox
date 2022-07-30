using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UISongPlay : UIEntity
{
	[SerializeField] UIGroup     m_ControlGroup;
	[SerializeField] UIGroup     m_LoaderGroup;
	[SerializeField] UIGroup     m_CompleteGroup;
	[SerializeField] UILevel     m_Level;
	[SerializeField] UIUnitLabel m_Coins;
	[SerializeField] GameObject  m_Lock;
	[SerializeField] GameObject  m_Free;
	[SerializeField] GameObject  m_Ads;
	[SerializeField] GameObject  m_Paid;
	[SerializeField] SongPreview m_Preview;

	[Inject] SongsManager      m_SongsManager;
	[Inject] SongsProcessor    m_SongsProcessor;
	[Inject] ProgressProcessor m_ProgressProcessor;
	[Inject] ProfileProcessor  m_ProfileProcessor;
	[Inject] AdsProcessor      m_AdsProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;

	string   m_SongID;
	SongMode m_Mode;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		m_Mode   = m_SongsProcessor.GetMode(m_SongID);
		
		if (m_SongsManager.IsSongAvailable(m_SongID))
			m_Mode = SongMode.Free;
		else if (m_Mode == SongMode.Ads && m_ProfileProcessor.HasNoAds())
			m_Mode = SongMode.Free;
		else if (m_Mode == SongMode.Paid && m_ProfileProcessor.HasSong(m_SongID))
			m_Mode = SongMode.Free;
		
		m_ControlGroup.Show(true);
		m_LoaderGroup.Hide(true);
		m_CompleteGroup.Hide(true);
		
		SetLockActive(false);
		SetFreeActive(false);
		SetAdsActive(false);
		SetPaidActive(false);
		
		if (m_SongsManager.IsSongLockedByLevel(m_SongID))
		{
			SetLockActive(true);
			
			return;
		}
		
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
				SetAdsActive(true);
				break;
		}
	}

	public void Play()
	{
		switch (m_Mode)
		{
			case SongMode.Free:
				PlayFree();
				break;
			case SongMode.Ads:
				PlayAds();
				break;
			case SongMode.Paid:
				PlayPaid();
				break;
			default:
				PlayFree();
				break;
		}
	}

	async void PlayFree()
	{
		if (!m_SongsManager.IsSongAvailable(m_SongID))
		{
			if (m_SongsManager.IsSongLockedByAds(m_SongID))
				PlayAds();
			else if (m_SongsManager.IsSongLockedByCoins(m_SongID))
				PlayPaid();
			
			return;
		}
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		StopPreview();
		
		UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
		
		loadingMenu.Setup(m_SongID);
		
		await m_MenuProcessor.Show(MenuType.LoadingMenu);
		
		loadingMenu.Load();
		
		await m_MenuProcessor.Hide(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.SongMenu, true);
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	async void PlayAds()
	{
		if (m_SongsManager.IsSongAvailable(m_SongID))
		{
			PlayFree();
			return;
		}
		
		string songID = m_SongID;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_ControlGroup.HideAsync();
		await m_LoaderGroup.ShowAsync();
		
		m_ProfileProcessor.Lock();
		
		bool success = false;
		
		if (m_ProfileProcessor.HasNoAds() || await m_AdsProcessor.Rewarded())
		{
			SongUnlockRequest request = new SongUnlockRequest(songID);
			
			success = await request.SendAsync();
		}
		
		if (success)
		{
			await m_LoaderGroup.HideAsync();
			await m_CompleteGroup.ShowAsync();
			
			await Task.Delay(500);
			
			StopPreview();
			
			UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
			
			loadingMenu.Setup(songID);
			
			await m_MenuProcessor.Show(MenuType.LoadingMenu);
			
			m_CompleteGroup.Hide(true);
			
			m_ProfileProcessor.Unlock();
			
			loadingMenu.Load();
			
			await m_MenuProcessor.Hide(MenuType.MainMenu, true);
			await m_MenuProcessor.Hide(MenuType.SongMenu, true);
		}
		else
		{
			m_ProfileProcessor.Unlock();
			
			await m_LoaderGroup.HideAsync();
			await m_ControlGroup.ShowAsync();
			
			await m_MenuProcessor.RetryLocalizedAsync(
				"song_play_ads",
				"song_play_button",
				"SONG_PLAY_ADS_ERROR_TITLE",
				"COMMON_ERROR_MESSAGE",
				PlayAds,
				() => { }
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	async void PlayPaid()
	{
		if (m_SongsManager.IsSongAvailable(m_SongID))
		{
			PlayFree();
			return;
		}
		
		string songID = m_SongID;
		
		long coins = m_SongsProcessor.GetPrice(songID);
		
		if (!await m_ProfileProcessor.CheckCoins(coins))
			return;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_ControlGroup.HideAsync();
		await m_LoaderGroup.ShowAsync();
		
		m_ProfileProcessor.Lock();
		
		SongUnlockRequest request = new SongUnlockRequest(songID);
		
		bool success = await request.SendAsync();
		
		if (success)
		{
			await m_LoaderGroup.HideAsync();
			await m_CompleteGroup.ShowAsync();
			
			await Task.Delay(500);
			
			StopPreview();
			
			UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
			
			loadingMenu.Setup(songID);
			
			await m_MenuProcessor.Show(MenuType.LoadingMenu);
			
			m_CompleteGroup.Hide(true);
			
			m_ProfileProcessor.Unlock();
			
			loadingMenu.Load();
			
			await m_MenuProcessor.Hide(MenuType.MainMenu, true);
			await m_MenuProcessor.Hide(MenuType.SongMenu, true);
		}
		else
		{
			m_ProfileProcessor.Unlock();
			
			await m_LoaderGroup.HideAsync();
			await m_ControlGroup.ShowAsync();
			
			await m_MenuProcessor.RetryLocalizedAsync(
				"song_play_paid",
				"song_play_button",
				"SONG_PLAY_PAID_ERROR_TITLE",
				"COMMON_ERROR_MESSAGE",
				PlayPaid,
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

	void SetLockActive(bool _Value)
	{
		if (m_Lock != null)
			m_Lock.SetActive(_Value);
		
		if (m_Level != null)
			m_Level.Level = m_ProgressProcessor.GetSongLevel(m_SongID);
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