using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

using ColorMode = UISongFrame.ColorMode;

public class UIMainMenuSongsPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Songs;

	[SerializeField] RectTransform m_Container;

	[Inject] SignalBus         m_SignalBus;
	[Inject] SongsManager      m_SongsManager;
	[Inject] ConfigProcessor   m_ConfigProcessor;
	[Inject] UISongHeader.Pool m_HeaderPool;
	[Inject] UISongFooter.Pool m_FooterPool;
	[Inject] UISongGroup.Pool  m_GroupPool;
	[Inject] UISongList.Pool   m_ListPool;

	readonly List<UISongHeader> m_Headers = new List<UISongHeader>();
	readonly List<UISongFooter> m_Footers = new List<UISongFooter>();
	readonly List<UISongGroup>  m_Groups  = new List<UISongGroup>();
	readonly List<UISongList>   m_Lists   = new List<UISongList>();

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
		Clear();
		
		CreateLibrary();
		
		CreateCoinsLocked();
		
		CreateLevelLocked();
	}

	void CreateLibrary()
	{
		List<string> songIDs = m_SongsManager.GetLibrarySongIDs();
		
		int size = m_ConfigProcessor.SongLibraryGroupSize;
		
		CreateGroup(songIDs.Take(size).ToArray());
		
		CreateList(songIDs.Skip(size).ToArray());
	}

	void CreateCoinsLocked()
	{
		List<string> songIDs = m_SongsManager.GetCoinsSongIDs();
		
		if (songIDs == null || songIDs.Count == 0)
			return;
		
		CreateHeader(
			ColorMode.Blue,
			GetLocalization("SONG_GROUP_COINS", "<sprite name=coins_icon>")
		);
		
		CreateList(songIDs);
		
		CreateFooter(ColorMode.Blue);
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
			
			CreateHeader(
				ColorMode.Red,
				GetLocalization("SONG_GROUP_LEVEL", $"<sprite name=level_{group.Key}>")
			);
			
			CreateList(group.Value);
			
			CreateFooter(ColorMode.Red);
		}
	}

	void CreateHeader(ColorMode _ColorMode, string _Title)
	{
		if (string.IsNullOrEmpty(_Title))
			return;
		
		UISongHeader item = m_HeaderPool.Spawn(m_Container);
		
		if (item == null)
			return;
		
		item.Mode = _ColorMode;
		
		item.Setup(_Title);
		
		m_Headers.Add(item);
	}

	void CreateFooter(ColorMode _ColorMode)
	{
		UISongFooter item = m_FooterPool.Spawn(m_Container);
		
		if (item == null)
			return;
		
		item.Mode = _ColorMode;
		
		m_Footers.Add(item);
	}

	void CreateGroup(ICollection<string> _SongIDs)
	{
		if (_SongIDs == null || _SongIDs.Count == 0)
			return;
		
		UISongGroup item = m_GroupPool.Spawn(m_Container);
		
		item.Setup(_SongIDs);
		
		m_Groups.Add(item);
	}

	void CreateList(ICollection<string> _SongIDs)
	{
		if (_SongIDs == null || _SongIDs.Count == 0)
			return;
		
		UISongList item = m_ListPool.Spawn(m_Container);
		
		item.Setup(_SongIDs);
		
		m_Lists.Add(item);
	}

	void Clear()
	{
		foreach (UISongHeader item in m_Headers)
			m_HeaderPool.Despawn(item);
		m_Headers.Clear();
		
		foreach (UISongFooter item in m_Footers)
			m_FooterPool.Despawn(item);
		m_Footers.Clear();
		
		foreach (UISongGroup item in m_Groups)
			m_GroupPool.Despawn(item);
		m_Groups.Clear();
		
		foreach (UISongList item in m_Lists)
			m_ListPool.Despawn(item);
		m_Lists.Clear();
	}
}