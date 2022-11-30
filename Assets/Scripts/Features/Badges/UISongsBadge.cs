using System.Collections.Generic;
using System.Linq;
using Zenject;

public class UISongsBadge : UIBadge
{
	public const string SONGS_GROUP = "songs";

	const string PRODUCTS_GROUP = "products";

	[Inject] SongsManager    m_SongsManager;
	[Inject] ProductsManager m_ProductsManager;

	protected override void Subscribe()
	{
		m_SongsManager.Profile.Subscribe(DataEventType.Add, Process);
		m_SongsManager.Profile.Subscribe(DataEventType.Remove, Process);
		m_SongsManager.Profile.Subscribe(DataEventType.Change, Process);
		m_SongsManager.Collection.Subscribe(DataEventType.Change, Process);
	}

	protected override void Unsubscribe()
	{
		m_SongsManager.Profile.Unsubscribe(DataEventType.Add, Process);
		m_SongsManager.Profile.Unsubscribe(DataEventType.Remove, Process);
		m_SongsManager.Profile.Unsubscribe(DataEventType.Change, Process);
		m_SongsManager.Collection.Unsubscribe(DataEventType.Change, Process);
	}

	protected override void Process()
	{
		int value = 0;
		
		value += GetSongsCount();
		value += GetProductsCount();
		
		SetValue(value);
	}

	int GetSongsCount()
	{
		List<string> songIDs = m_SongsManager.GetAvailableSongIDs();
		
		return songIDs?.Count(_SongID => BadgeManager.IsUnread(SONGS_GROUP, _SongID)) ?? 0;
	}

	int GetProductsCount()
	{
		List<string> productIDs = m_ProductsManager.GetRecommendedProductIDs(3);
		
		return productIDs?.Count(_ProductID => BadgeManager.IsUnread(PRODUCTS_GROUP, _ProductID)) ?? 0;
	}
}
