using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class UIMainMenuSongsPage : UIMainMenuPage
{
	const float ITEM_ASPECT  = 0.675f;
	const float GRID_SPACING = 25;
	const float LIST_SPACING = 20;

	public override MainMenuPageType Type => MainMenuPageType.Songs;

	[SerializeField] UILayout m_Content;
	[SerializeField] UIGroup  m_ContentGroup;
	[SerializeField] UIGroup  m_LoaderGroup;

	[Inject] AudioManager    m_AudioManager;
	[Inject] SongsManager    m_SongsManager;
	[Inject] ProductsManager m_ProductsManager;
	[Inject] FramesManager   m_FramesManager;
	[Inject] SocialProcessor m_SocialProcessor;

	[Inject] ConfigProcessor m_ConfigProcessor;
	[Inject] MenuProcessor   m_MenuProcessor;

	[Inject] UIAdminElement.Pool        m_AdminPool;
	[Inject] UIProductCoinsElement.Pool m_ProductPool;
	[Inject] UISongItem.Pool            m_SongsPool;
	[Inject] UISongElement.Pool         m_ElementPool;
	[Inject] UILatencyElement.Factory   m_LatencyFactory;
	[Inject] UIAmbientElement.Pool      m_AmbientPool;
	[Inject] UIFrameElement.Pool        m_FramesPool;

	protected override async void OnShowStarted()
	{
		m_ContentGroup.Hide(true);
		m_LoaderGroup.Show(true);
		
		bool instant = true;
		
		instant &= await m_SongsManager.Activate();
		instant &= await m_ProductsManager.Activate();
		
		if (!IsActiveSelf)
			return;
		
		m_ContentGroup.Show(instant);
		m_LoaderGroup.Hide(instant);
		
		Refresh();
		
		m_SongsManager.Collection.Subscribe(DataEventType.Add, Refresh);
		m_SongsManager.Collection.Subscribe(DataEventType.Remove, Refresh);
		m_ProductsManager.Collection.Subscribe(DataEventType.Add, Refresh);
		m_ProductsManager.Collection.Subscribe(DataEventType.Remove, Refresh);
	}

	protected override void OnHideStarted()
	{
		m_SongsManager.Collection.Unsubscribe(DataEventType.Add, Refresh);
		m_SongsManager.Collection.Unsubscribe(DataEventType.Remove, Refresh);
		m_ProductsManager.Collection.Unsubscribe(DataEventType.Add, Refresh);
		m_ProductsManager.Collection.Unsubscribe(DataEventType.Remove, Refresh);
	}

	void Refresh()
	{
		m_Content.Clear();
		
		CreateAdminPanel();
		
		CreateAmbient();
		
		CreateFrames();
		
		CreateLibrary();
		
		CreatePaidSongs();
		
		CreateChestSongs();
		
		m_Content.Reposition();
	}

	void CreateAdminPanel()
	{
		if (!AdminMode.Enabled)
			return;
		
		AdminElementEntity admin = new AdminElementEntity(
			"Admin",
			() =>
			{
				UIAdminMenu adminMenu = m_MenuProcessor.GetMenu<UIAdminMenu>();
				
				if (adminMenu == null)
					return;
				
				adminMenu.Show();
			},
			m_AdminPool
		);
		
		AdminElementEntity maps = new AdminElementEntity(
			"Maps",
			() =>
			{
				UIMapsMenu mapsMenu = m_MenuProcessor.GetMenu<UIMapsMenu>();
				
				if (mapsMenu == null)
					return;
				
				mapsMenu.Show();
			},
			m_AdminPool
		);
		
		AdminElementEntity login = new AdminElementEntity(
			"Login",
			LoginAdmin,
			m_AdminPool
		);
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(admin);
		
		m_Content.Add(maps);
		
		m_Content.Add(login);
		
		VerticalStackLayout.End(m_Content);
	}

	async void LoginAdmin()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_SocialProcessor.AttachEmail("mityugovmaxim@gmail.com", "121SuperMaxim");
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	void CreateAmbient()
	{
		m_Content.Spacing(LIST_SPACING);
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(new AmbientElementEntity(m_AmbientPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateFrames()
	{
		List<string> frameIDs = m_FramesManager.GetFrameIDs();
		
		if (frameIDs == null || frameIDs.Count == 0)
			return;
		
		m_Content.Spacing(LIST_SPACING);
		
		VerticalGridLayout.Start(m_Content, 4, 1, 10, 10);
		
		foreach (string frameID in frameIDs)
			m_Content.Add(new FrameElementEntity(frameID, m_FramesPool));
		
		VerticalGridLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateLibrary()
	{
		List<string> songIDs = m_SongsManager.GetAvailableSongIDs();
		
		if (songIDs == null || songIDs.Count == 0)
			return;
		
		CreateLatency();
		
		int group = m_ConfigProcessor.SongLibraryGroupSize;
		
		int position = 0;
		
		position += CreateGrid(songIDs, position, group);
		
		position += CreateList(songIDs, position, 3);
		
		CreateCoins();
		
		CreateList(songIDs, position);
	}

	void CreatePaidSongs()
	{
		List<string> songIDs = m_SongsManager.GetPaidSongIDs();
		
		if (songIDs == null || songIDs.Count == 0)
			return;
		
		CreateList(songIDs);
	}

	void CreateChestSongs()
	{
		List<string> songIDs = m_SongsManager.GetChestSongIDs();
		
		if (songIDs == null || songIDs.Count == 0)
			return;
		
		CreateList(songIDs);
	}

	void CreateLatency()
	{
		if (m_AudioManager.HasSettings() || m_AudioManager.GetAudioOutputType() != AudioOutputType.Bluetooth)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(new LatencyElementEntity(m_LatencyFactory));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	int CreateGrid(List<string> _SongIDs, int _Skip = 0, int _Take = 0)
	{
		if (_SongIDs == null || _SongIDs.Count == 0)
			return 0;
		
		List<string> songIDs;
		
		if (_Skip > 0 && _Take > 0)
			songIDs = _SongIDs.Skip(_Skip).Take(_Take).ToList();
		else if (_Skip > 0)
			songIDs = _SongIDs.Skip(_Skip).ToList();
		else if (_Take > 0)
			songIDs = _SongIDs.Take(_Take).ToList();
		else
			songIDs = _SongIDs;
		
		if (songIDs.Count == 0)
			return 0;
		
		m_Content.Spacing(LIST_SPACING);
		
		VerticalGridLayout.Start(m_Content, 2, 1, GRID_SPACING, GRID_SPACING);
		
		int count = 0;
		foreach (string songID in songIDs)
		{
			m_Content.Add(new SongItemEntity(songID, m_SongsPool));
			count++;
		}
		
		VerticalGridLayout.End(m_Content);
		
		return count;
	}

	int CreateList(List<string> _SongIDs, int _Skip = 0, int _Take = 0)
	{
		if (_SongIDs == null || _SongIDs.Count == 0)
			return 0;
		
		List<string> songIDs;
		
		if (_Skip > 0 && _Take > 0)
			songIDs = _SongIDs.Skip(_Skip).Take(_Take).ToList();
		else if (_Skip > 0)
			songIDs = _SongIDs.Skip(_Skip).ToList();
		else if (_Take > 0)
			songIDs = _SongIDs.Take(_Take).ToList();
		else
			songIDs = _SongIDs;
		
		if (songIDs.Count == 0)
			return 0;
		
		m_Content.Spacing(LIST_SPACING);
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		int count = 0;
		foreach (string songID in songIDs)
		{
			m_Content.Add(new SongElementEntity(songID, m_ElementPool));
			count++;
		}
		
		VerticalStackLayout.End(m_Content);
		
		return count;
	}

	void CreateCoins()
	{
		const int count = 3;
		
		List<string> coinsIDs = m_ProductsManager.GetRecommendedProductIDs(count);
		
		if (coinsIDs == null || coinsIDs.Count < count)
			return;
		
		m_Content.Spacing(LIST_SPACING);
		
		VerticalGridLayout.Start(m_Content, count, ITEM_ASPECT, GRID_SPACING / 2, GRID_SPACING);
		
		foreach (string coinsID in coinsIDs.Take(count))
			m_Content.Add(new ProductCoinsElementEntity(coinsID, m_ProductPool));
		
		VerticalGridLayout.End(m_Content);
	}
}
