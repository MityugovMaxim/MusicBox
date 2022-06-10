using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

using ColorMode = UIStroke.ColorMode;

public class UIMainMenuSongsPage : UIMainMenuPage
{
	const float GRID_SPACING = 30;
	const float LIST_SPACING = 15;

	public override MainMenuPageType Type => MainMenuPageType.Songs;

	[SerializeField] UILayout m_Content;

	[Inject] SignalBus          m_SignalBus;
	[Inject] SongsManager       m_SongsManager;
	[Inject] ConfigProcessor    m_ConfigProcessor;
	[Inject] UISongHeader.Pool  m_HeaderPool;
	[Inject] UISongItem.Pool    m_ItemPool;
	[Inject] UISongElement.Pool m_ElementPool;

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
		
		int size = m_ConfigProcessor.SongLibraryGroupSize;
		
		VerticalGridLayout.Start(m_Content, 2, 1, GRID_SPACING, GRID_SPACING);
		
		foreach (string songID in songIDs.Take(size))
			m_Content.Add(new SongItemEntity(songID, m_ItemPool));
		
		m_Content.Space(LIST_SPACING);
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (string songID in songIDs.Skip(size))
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
		
		foreach (string songID in songIDs)
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