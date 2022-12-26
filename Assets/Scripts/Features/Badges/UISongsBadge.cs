using System.Collections.Generic;
using System.Linq;
using Zenject;

public class UISongsBadge : UIBadge
{
	[Inject] SongsManager    m_SongsManager;
	[Inject] ProductsManager m_ProductsManager;

	protected override void Subscribe()
	{
		BadgeManager.SubscribeSongs(Process);
		BadgeManager.SubscribeProducts(Process);
		m_SongsManager.Profile.Subscribe(DataEventType.Add, Process);
		m_SongsManager.Profile.Subscribe(DataEventType.Remove, Process);
		m_ProductsManager.Collection.Subscribe(DataEventType.Add, Process);
		m_ProductsManager.Collection.Subscribe(DataEventType.Remove, Process);
	}

	protected override void Unsubscribe()
	{
		BadgeManager.UnsubscribeSongs(Process);
		BadgeManager.UnsubscribeProducts(Process);
		m_SongsManager.Profile.Unsubscribe(DataEventType.Add, Process);
		m_SongsManager.Profile.Unsubscribe(DataEventType.Remove, Process);
		m_ProductsManager.Collection.Unsubscribe(DataEventType.Add, Process);
		m_ProductsManager.Collection.Unsubscribe(DataEventType.Remove, Process);
	}

	protected override async void Preload()
	{
		await m_SongsManager.Activate();
		
		base.Preload();
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
		
		return songIDs?.Count(_SongID => BadgeManager.IsSongUnread(_SongID)) ?? 0;
	}

	int GetProductsCount()
	{
		List<string> productIDs = m_ProductsManager.GetRecommendedProductIDs(3);
		
		return productIDs?.Count(_ProductID => BadgeManager.IsProductUnread(_ProductID)) ?? 0;
	}
}
