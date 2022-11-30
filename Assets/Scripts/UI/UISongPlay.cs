using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UISongPlay : UISongEntity
{
	[SerializeField] UIGroup    m_ControlGroup;
	[SerializeField] UIGroup    m_LoaderGroup;
	[SerializeField] UIFlare    m_Flare;
	[SerializeField] Button     m_FreeButton;
	[SerializeField] Button     m_PaidButton;
	[SerializeField] GameObject m_FreeContent;
	[SerializeField] GameObject m_PaidContent;

	[Inject] SongsManager          m_SongsManager;
	[Inject] ProfileCoinsParameter m_ProfileCoins;
	[Inject] MenuProcessor         m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_FreeButton.Subscribe(Play);
		m_PaidButton.Subscribe(Play);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_FreeButton.Unsubscribe(Play);
		m_PaidButton.Unsubscribe(Play);
	}

	protected override void Subscribe()
	{
		SongsManager.Profile.Subscribe(DataEventType.Add, SongID, ProcessData);
		SongsManager.Profile.Subscribe(DataEventType.Remove, SongID, ProcessData);
		SongsManager.Profile.Subscribe(DataEventType.Change, SongID, ProcessData);
		SongsManager.Collection.Subscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SongsManager.Profile.Unsubscribe(DataEventType.Add, SongID, ProcessData);
		SongsManager.Profile.Unsubscribe(DataEventType.Remove, SongID, ProcessData);
		SongsManager.Profile.Unsubscribe(DataEventType.Change, SongID, ProcessData);
		SongsManager.Collection.Unsubscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_ControlGroup.Show(true);
		m_LoaderGroup.Hide(true);
		
		SongMode mode = SongsManager.GetMode(SongID);
		
		m_FreeContent.SetActive(mode == SongMode.Free);
		m_PaidContent.SetActive(mode == SongMode.Paid);
	}

	void Play()
	{
		SongMode mode = SongsManager.GetMode(SongID);
		
		switch (mode)
		{
			case SongMode.Free:
				PlayFree();
				break;
			case SongMode.Paid:
				PlayPaid();
				break;
		}
	}

	async void PlayFree()
	{
		if (m_SongsManager.IsPaid(SongID))
		{
			PlayPaid();
			return;
		}
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
		
		loadingMenu.Setup(SongID);
		
		await m_MenuProcessor.Show(MenuType.LoadingMenu);
		
		loadingMenu.Load();
		
		await m_MenuProcessor.Hide(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.SongMenu, true);
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	async void PlayPaid()
	{
		if (m_SongsManager.IsAvailable(SongID))
		{
			PlayFree();
			return;
		}
		
		string songID = SongID;
		
		long coins = m_SongsManager.GetPrice(songID);
		
		if (!await m_ProfileCoins.Remove(coins))
			return;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_ControlGroup.HideAsync();
		await m_LoaderGroup.ShowAsync();
		
		SongUnlockRequest request = new SongUnlockRequest(songID);
		
		bool success = await request.SendAsync();
		
		if (success)
		{
			await m_LoaderGroup.HideAsync();
			
			m_Flare.Play();
			
			await Task.Delay(500);
			
			UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
			
			loadingMenu.Setup(songID);
			
			await m_MenuProcessor.Show(MenuType.LoadingMenu);
			
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
}
