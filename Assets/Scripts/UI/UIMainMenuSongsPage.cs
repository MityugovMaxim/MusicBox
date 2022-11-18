using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

using ColorMode = UIStroke.ColorMode;

public class UIMainMenuSongsPage : UIMainMenuPage
{
	const float ITEM_ASPECT  = 0.675f;
	const float GRID_SPACING = 30;
	const float LIST_SPACING = 15;

	public override MainMenuPageType Type => MainMenuPageType.Songs;

	[SerializeField] UILayout m_Content;

	[Inject] AudioManager    m_AudioManager;
	[Inject] SongsManager    m_SongsManager;
	[Inject] ProductsManager m_ProductsManager;

	[Inject] RolesProcessor  m_RolesProcessor;
	[Inject] ConfigProcessor m_ConfigProcessor;
	[Inject] SocialProcessor m_SocialProcessor;
	[Inject] MenuProcessor   m_MenuProcessor;

	[Inject] UIAdminElement.Pool      m_AdminPool;
	[Inject] UISocialElement.Pool     m_SocialPool;
	[Inject] UISongHeader.Pool        m_HeaderPool;
	[Inject] UIProductItem.Pool       m_ProductPool;
	[Inject] UIProductPromo.Pool      m_PromoPool;
	[Inject] UISongItem.Pool          m_ItemPool;
	[Inject] UISongElement.Pool       m_ElementPool;
	[Inject] UILatencyElement.Factory m_LatencyFactory;

	protected override void OnShowStarted()
	{
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
		
		if (AdminMode.Enabled)
		{
			CreateAdminRoles();
			CreateAdminSongs();
			CreateAdminMaps();
			CreateAdminProgress();
			CreateAdminAmbient();
			CreateAdminRevives();
			CreateAdminVouchers();
			CreateAdminLanguages();
			CreateAdminLocalizations();
		}
		
		CreateLibrary();
		
		CreateLevelLocked();
		
		m_Content.Reposition();
	}

	void CreateAdminRoles()
	{
		if (!m_RolesProcessor.HasRolesPermission())
			return;
		
		AdminElementEntity roles = new AdminElementEntity(
			"Edit roles",
			"roles",
			typeof(RoleSnapshot),
			m_AdminPool
		);
		
		CreateAdmin(roles);
	}

	void CreateAdminSongs()
	{
		if (!m_RolesProcessor.HasSongsPermission())
			return;
		
		AdminElementEntity songs = new AdminElementEntity(
			"Edit songs",
			"songs",
			typeof(SongSnapshot),
			m_AdminPool
		);
		
		CreateAdmin(songs);
	}

	void CreateAdminMaps()
	{
		if (!m_RolesProcessor.HasSongsPermission())
			return;
		
		AdminElementEntity maps = new AdminElementEntity(
			"Edit maps",
			m_AdminPool,
			() =>
			{
				UIMapsMenu mapsMenu = m_MenuProcessor.GetMenu<UIMapsMenu>();
				
				if (mapsMenu == null)
					return;
				
				mapsMenu.Show();
			}
		);
		
		CreateAdmin(maps);
	}

	void CreateAdminProgress()
	{
		if (!m_RolesProcessor.HasProgressPermission())
			return;
		
		AdminElementEntity progress = new AdminElementEntity(
			"Edit progress",
			"progress",
			typeof(ProgressSnapshot),
			m_AdminPool
		);
		
		CreateAdmin(progress);
	}

	void CreateAdminAmbient()
	{
		if (!m_RolesProcessor.HasAmbientPermission())
			return;
		
		AdminElementEntity ambient = new AdminElementEntity(
			"Edit ambient",
			"ambient",
			typeof(AmbientSnapshot),
			m_AdminPool
		);
		
		CreateAdmin(ambient);
	}

	void CreateAdminRevives()
	{
		if (!m_RolesProcessor.HasRevivesPermission())
			return;
		
		AdminElementEntity revives = new AdminElementEntity(
			"Edit revives",
			"revives",
			typeof(ReviveSnapshot),
			m_AdminPool
		);
		
		CreateAdmin(revives);
	}

	void CreateAdminVouchers()
	{
		if (!m_RolesProcessor.HasRolesPermission())
			return;
		
		AdminElementEntity vouchers = new AdminElementEntity(
			"Create voucher",
			m_AdminPool,
			() =>
			{
				UIVoucherCreateMenu voucherCreateMenu = m_MenuProcessor.GetMenu<UIVoucherCreateMenu>();
				
				if (voucherCreateMenu == null)
					return;
				
				voucherCreateMenu.Show();
			}
		);
		
		CreateAdmin(vouchers);
	}

	void CreateAdminLanguages()
	{
		if (!m_RolesProcessor.HasLanguagesPermission())
			return;
		
		AdminElementEntity languages = new AdminElementEntity(
			"Edit languages",
			"languages",
			typeof(LanguageSnapshot),
			m_AdminPool
		);
		
		CreateAdmin(languages);
	}

	void CreateAdminLocalizations()
	{
		if (!m_RolesProcessor.HasLanguagesPermission())
			return;
		
		AdminElementEntity localizations = new AdminElementEntity(
			"Edit localizations",
			m_AdminPool,
			() =>
			{
				UILanguagesMenu languagesMenu = m_MenuProcessor.GetMenu<UILanguagesMenu>();
				
				if (languagesMenu == null)
					return;
				
				languagesMenu.Show();
			}
		);
		
		CreateAdmin(localizations);
	}

	void CreateAdmin(AdminElementEntity _AdminElement)
	{
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(_AdminElement);
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateLibrary()
	{
		List<string> songIDs = m_SongsManager.GetLibrarySongIDs();
		
		if (songIDs == null || songIDs.Count == 0)
			return;
		
		CreateLatency();
		
		CreateSocial();
		
		int group = m_ConfigProcessor.SongLibraryGroupSize;
		
		int position = 0;
		
		position += CreateGrid(songIDs, position, group);
		
		position += CreateList(songIDs, position, 2);
		
		CreatePromo();
		
		position += CreateList(songIDs, position, 4);
		
		CreateCoins();
		
		CreateList(songIDs, position);
	}

	void CreateLevelLocked()
	{
		Dictionary<int, List<string>> groups = m_SongsManager.GetLockedSongIDs();
		
		if (groups == null || groups.Count == 0)
			return;
		
		foreach (var group in groups)
		{
			if (group.Value == null || group.Value.Count == 0)
				continue;
			
			string title = GetLocalization("SONG_GROUP_LEVEL", $"<sprite name=level_{group.Key}>");
			
			CreateHeader(title, ColorMode.Red);
			
			int position = 0;
			
			position += CreateGrid(group.Value, position, 2);
			
			CreateList(group.Value, position);
		}
	}

	void CreateSocial()
	{
		if (!m_SocialProcessor.Guest)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(new SocialElementEntity(m_SocialPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
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

	void CreateHeader(string _Title, ColorMode _Color)
	{
		m_Content.Space(LIST_SPACING);
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(new SongHeaderEntity(_Title, _Color, m_HeaderPool));
		
		VerticalStackLayout.End(m_Content);
	}

	int CreateGrid(List<string> _SongIDs, int _Skip = 0, int _Take = 0)
	{
		VerticalGridLayout.Start(m_Content, 2, 1, GRID_SPACING, GRID_SPACING);
		
		IEnumerable<string> songIDs;
		
		if (_Skip > 0 && _Take > 0)
			songIDs = _SongIDs.Skip(_Skip).Take(_Take);
		else if (_Skip > 0)
			songIDs = _SongIDs.Skip(_Skip);
		else if (_Take > 0)
			songIDs = _SongIDs.Take(_Take);
		else
			songIDs = _SongIDs;
		
		int count = 0;
		foreach (string songID in songIDs)
		{
			m_Content.Add(new SongItemEntity(songID, m_ItemPool));
			count++;
		}
		
		VerticalGridLayout.End(m_Content);
		
		if (count > 0)
			m_Content.Space(LIST_SPACING);
		
		return count;
	}

	int CreateList(List<string> _SongIDs, int _Skip = 0, int _Take = 0)
	{
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		IEnumerable<string> songIDs;
		
		if (_Skip > 0 && _Take > 0)
			songIDs = _SongIDs.Skip(_Skip).Take(_Take);
		else if (_Skip > 0)
			songIDs = _SongIDs.Skip(_Skip);
		else if (_Take > 0)
			songIDs = _SongIDs.Take(_Take);
		else
			songIDs = _SongIDs;
		
		int count = 0;
		foreach (string songID in songIDs)
		{
			m_Content.Add(new SongElementEntity(songID, m_ElementPool));
			count++;
		}
		
		VerticalStackLayout.End(m_Content);
		
		if (count > 0)
			m_Content.Space(LIST_SPACING);
		
		return count;
	}

	void CreatePromo()
	{
		string promoID = m_ProductsManager.GetPromoProductIDs().FirstOrDefault();
		
		if (string.IsNullOrEmpty(promoID))
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(new ProductPromoEntity(promoID, m_PromoPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateCoins()
	{
		const int count = 3;
		
		List<string> coinsIDs = m_ProductsManager.GetRecommendedProductIDs(count);
		
		if (coinsIDs == null || coinsIDs.Count < count)
			return;
		
		VerticalGridLayout.Start(m_Content, count, ITEM_ASPECT, GRID_SPACING / 2, GRID_SPACING);
		
		foreach (string coinsID in coinsIDs.Take(count))
			m_Content.Add(new ProductItemEntity(coinsID, m_ProductPool));
		
		VerticalGridLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}
}
