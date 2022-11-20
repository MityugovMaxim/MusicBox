using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UISongPlay : UIEntity
{
	[SerializeField] UIGroup     m_ControlGroup;
	[SerializeField] UIGroup     m_LoaderGroup;
	[SerializeField] UIGroup     m_CompleteGroup;
	[SerializeField] UIUnitLabel m_Coins;
	[SerializeField] GameObject  m_Free;
	[SerializeField] GameObject  m_Ads;
	[SerializeField] GameObject  m_Paid;
	[SerializeField] SongPreview m_Preview;

	[Inject] SongsManager   m_SongsManager;
	[Inject] CoinsParameter m_CoinsParameter;
	[Inject] MenuProcessor  m_MenuProcessor;

	string   m_SongID;
	SongMode m_Mode;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_ControlGroup.Show(true);
		m_LoaderGroup.Hide(true);
		m_CompleteGroup.Hide(true);
		
		SetFreeActive(false);
		SetAdsActive(false);
		SetPaidActive(false);
		
		switch (m_Mode)
		{
			case SongMode.Free:
				SetFreeActive(true);
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

	async void PlayPaid()
	{
		if (m_SongsManager.IsSongAvailable(m_SongID))
		{
			PlayFree();
			return;
		}
		
		string songID = m_SongID;
		
		long coins = m_SongsManager.GetPrice(songID);
		
		if (!await m_CoinsParameter.Remove(coins))
			return;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_ControlGroup.HideAsync();
		await m_LoaderGroup.ShowAsync();
		
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
			
			loadingMenu.Load();
			
			await m_MenuProcessor.Hide(MenuType.MainMenu, true);
			await m_MenuProcessor.Hide(MenuType.SongMenu, true);
		}
		else
		{
			await m_LoaderGroup.HideAsync();
			await m_ControlGroup.ShowAsync();
			
			await m_MenuProcessor.RetryAsync(
				"song_unlock",
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
			m_Coins.Value = m_SongsManager.GetPrice(m_SongID);
	}
}
