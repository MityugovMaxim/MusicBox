using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIMainSongsPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Songs;

	[SerializeField] RectTransform m_Container;

	[Inject] SignalBus        m_SignalBus;
	[Inject] SongsManager     m_SongsManager;
	[Inject] UISongGroup.Pool m_ItemPool;

	readonly List<UISongGroup> m_Items = new List<UISongGroup>();

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
		
		CreateLibrary();
		
		CreateProductsLocked();
		
		CreateCoinsLocked();
		
		CreateLevelLocked();
	}

	void CreateLibrary()
	{
		List<string> songIDs = m_SongsManager.GetLibrarySongIDs();
		
		CreateItems(string.Empty, songIDs);
	}

	void CreateProductsLocked()
	{
		List<string> songIDs = m_SongsManager.GetProductSongIDs();
		
		CreateItems(GetLocalization("SONGS_PRODUCTS"), songIDs);
	}

	void CreateCoinsLocked()
	{
		List<string> songIDs = m_SongsManager.GetCoinsSongIDs();
		
		CreateItems(GetLocalization("SONGS_COINS", "<sprite name=coins_icon>"), songIDs);
	}

	void CreateLevelLocked()
	{
		Dictionary<int, string[]> groups = m_SongsManager.GetLockedSongIDs();
		
		foreach (var group in groups)
			CreateItems(GetLocalization("SONGS_LEVEL", $"<sprite name=level_{group.Key}>"), group.Value);
	}

	void CreateItems(string _Title, ICollection<string> _SongIDs)
	{
		if (_SongIDs == null || _SongIDs.Count == 0)
			return;
		
		UISongGroup item = m_ItemPool.Spawn(m_Container);
		
		item.Setup(_Title, _SongIDs);
		
		m_Items.Add(item);
	}
}