using System;
using System.Collections.Generic;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

public class UIMainSongsPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Songs;

	[SerializeField] RectTransform m_Container;
	[SerializeField] UIEntity      m_Control;

	[Inject] SignalBus        m_SignalBus;
	[Inject] SongsProcessor   m_SongsProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;
	[Inject] SongsManager     m_SongsManager;
	[Inject] UISongGroup.Pool m_ItemPool;

	readonly List<UISongGroup> m_Items = new List<UISongGroup>();

	public void CreateSong()
	{
		m_SongsProcessor.CreateSnapshot();
		
		Refresh();
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_SongsProcessor.Upload();
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload songs failed.");
			
			string message = exception.GetBaseException().Message;
			
			await m_MenuProcessor.ErrorAsync(
				"upload_songs",
				"Upload failed",
				message
			);
		}
		
		Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_SongsProcessor.Load();
		
		Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	protected override void OnShowStarted()
	{
		Refresh();
		
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<SongsDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ScoresDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProductsDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<SongsDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ScoresDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProductsDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		foreach (UISongGroup item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		Create();
		
		//CreateLibrary();
		
		//CreateCoinsLocked();
		
		//CreateProductsLocked();
		
		//CreateLevelLocked();
		
		m_Control.BringToFront();
	}

	void Create()
	{
		List<string> songIDs = m_SongsManager.GetSongIDs();
		
		CreateItems(string.Empty, songIDs);
	}

	// void CreateLibrary()
	// {
	// 	List<string> songIDs = m_SongsManager.GetLibrarySongIDs();
	// 	
	// 	CreateItems(string.Empty, songIDs);
	// }
	//
	// void CreateCoinsLocked()
	// {
	// 	List<string> songIDs = m_SongsManager.GetCoinsSongIDs();
	// 	
	// 	CreateItems(GetLocalization("SONG_GROUP_COINS", "<sprite name=coins_icon>"), songIDs);
	// }
	//
	// void CreateProductsLocked()
	// {
	// 	List<string> songIDs = m_SongsManager.GetProductSongIDs();
	// 	
	// 	CreateItems(GetLocalization("SONG_GROUP_PRODUCTS", "<sprite name=shop_icon>"), songIDs);
	// }
	//
	// void CreateLevelLocked()
	// {
	// 	Dictionary<int, string[]> groups = m_SongsManager.GetLockedSongIDs();
	// 	
	// 	foreach (var group in groups)
	// 		CreateItems(GetLocalization("SONG_GROUP_LEVEL", $"<sprite name=level_{group.Key}>"), group.Value);
	// }

	void CreateItems(string _Title, ICollection<string> _SongIDs)
	{
		if (_SongIDs == null || _SongIDs.Count == 0)
			return;
		
		UISongGroup item = m_ItemPool.Spawn(m_Container);
		
		item.Setup(_Title, _SongIDs);
		
		m_Items.Add(item);
	}
}