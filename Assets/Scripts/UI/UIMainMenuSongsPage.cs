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

	[Inject] SignalBus       m_SignalBus;
	[Inject] SongsManager    m_SongsManager;
	[Inject] ProductsManager m_ProductsManager;
	[Inject] ConfigProcessor m_ConfigProcessor;
	[Inject] SocialProcessor m_SocialProcessor;

	[Inject] UISocialElement.Pool m_SocialPool;
	[Inject] UISongHeader.Pool    m_HeaderPool;
	[Inject] UIProductItem.Pool   m_ProductPool;
	[Inject] UIProductPromo.Pool  m_PromoPool;
	[Inject] UISongItem.Pool      m_ItemPool;
	[Inject] UISongElement.Pool   m_ElementPool;

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
		if (m_SignalBus == null)
			return;
		
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<SongsDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ScoresDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProductsDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		m_Content.Clear();
		
		CreateLibrary();
		
		CreateCoinsLocked();
		
		CreateLevelLocked();
		
		m_Content.Reposition();
	}

	void CreateLibrary()
	{
		List<string> songIDs = m_SongsManager.GetLibrarySongIDs();
		
		if (songIDs == null || songIDs.Count == 0)
			return;
		
		if (m_SocialProcessor.Guest)
		{
			VerticalStackLayout.Start(m_Content, LIST_SPACING);
			
			m_Content.Add(new SocialElementEntity(m_SocialPool));
			
			m_Content.Space(LIST_SPACING);
		}
		
		int size = m_ConfigProcessor.SongLibraryGroupSize;
		
		VerticalGridLayout.Start(m_Content, 2, 1, GRID_SPACING, GRID_SPACING);
		
		foreach (string songID in songIDs.Take(size))
			m_Content.Add(new SongItemEntity(songID, m_ItemPool));
		
		m_Content.Space(LIST_SPACING);
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (string songID in songIDs.Skip(size).Take(2))
			m_Content.Add(new SongElementEntity(songID, m_ElementPool));
		
		string productID = m_ProductsManager.GetPromoProductIDs().FirstOrDefault();
		
		if (!string.IsNullOrEmpty(productID))
			m_Content.Add(new ProductPromoEntity(productID, m_PromoPool));
		
		foreach (string songID in songIDs.Skip(size + 2))
			m_Content.Add(new SongElementEntity(songID, m_ElementPool));
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateCoinsLocked()
	{
		List<string> songIDs = m_SongsManager.GetCoinsSongIDs();
		
		if (songIDs == null || songIDs.Count == 0)
			return;
		
		m_Content.Space(LIST_SPACING);
		
		VerticalStackLayout.Start(m_Content, 0);
		
		string title = GetLocalization("SONG_GROUP_COINS", "<sprite name=coins_icon>");
		
		m_Content.Add(new SongHeaderEntity(title, ColorMode.Blue, m_HeaderPool));
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		const int productsPosition = 6;
		
		foreach (string songID in songIDs.Take(productsPosition))
			m_Content.Add(new SongElementEntity(songID, m_ElementPool));
		
		List<string> productIDs = m_ProductsManager.GetAvailableProductIDs();
		
		if (productIDs != null && productIDs.Count >= 3)
		{
			m_Content.Space(LIST_SPACING);
			
			VerticalGridLayout.Start(m_Content, 3, ITEM_ASPECT, GRID_SPACING / 2, GRID_SPACING);
			
			foreach (string productID in productIDs.Take(3))
				m_Content.Add(new ProductItemEntity(productID, m_ProductPool));
			
			if (songIDs.Count > productsPosition)
				m_Content.Space(LIST_SPACING);
			
			VerticalStackLayout.Start(m_Content, LIST_SPACING);
		}
		
		foreach (string songID in songIDs.Skip(productsPosition))
			m_Content.Add(new SongElementEntity(songID, m_ElementPool));
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateLevelLocked()
	{
		Dictionary<int, string[]> groups = m_SongsManager.GetLockedSongIDs();
		
		if (groups == null || groups.Count == 0)
			return;
		
		foreach (var group in groups)
		{
			if (group.Value == null || group.Value.Length == 0)
				continue;
			
			m_Content.Space(LIST_SPACING);
			
			VerticalStackLayout.Start(m_Content, 0);
			
			string title = GetLocalization("SONG_GROUP_LEVEL", $"<sprite name=level_{group.Key}>");
			
			m_Content.Add(new SongHeaderEntity(title, ColorMode.Red, m_HeaderPool));
			
			VerticalStackLayout.Start(m_Content, LIST_SPACING);
			
			foreach (string songID in group.Value)
				m_Content.Add(new SongElementEntity(songID, m_ElementPool));
			
			m_Content.Space(LIST_SPACING);
		}
	}
}