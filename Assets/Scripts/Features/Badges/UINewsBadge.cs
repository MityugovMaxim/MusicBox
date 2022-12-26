using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zenject;

public class UIProfileBadge : UIBadge
{
	[Inject] VouchersManager m_VouchersManager;

	protected override void Subscribe()
	{
		throw new System.NotImplementedException();
	}

	protected override void Unsubscribe()
	{
		throw new System.NotImplementedException();
	}

	protected override void Process()
	{
		throw new System.NotImplementedException();
	}
}

public class UINewsBadge : UIBadge
{
	[Inject] NewsManager   m_NewsManager;
	[Inject] OffersManager m_OffersManager;

	protected override void Subscribe()
	{
		BadgeManager.SubscribeNews(Process);
		m_NewsManager.Collection.Subscribe(DataEventType.Add, Process);
		m_NewsManager.Collection.Subscribe(DataEventType.Remove, Process);
		m_OffersManager.Profile.Subscribe(DataEventType.Add, Process);
		m_OffersManager.Profile.Subscribe(DataEventType.Remove, Process);
		m_OffersManager.Profile.Subscribe(DataEventType.Change, Process);
		m_OffersManager.Collection.Subscribe(DataEventType.Add, Process);
		m_OffersManager.Collection.Subscribe(DataEventType.Remove, Process);
	}

	protected override void Unsubscribe()
	{
		BadgeManager.UnsubscribeNews(Process);
		m_NewsManager.Collection.Unsubscribe(DataEventType.Add, Process);
		m_NewsManager.Collection.Unsubscribe(DataEventType.Remove, Process);
		m_OffersManager.Profile.Unsubscribe(DataEventType.Add, Process);
		m_OffersManager.Profile.Unsubscribe(DataEventType.Remove, Process);
		m_OffersManager.Profile.Unsubscribe(DataEventType.Change, Process);
		m_OffersManager.Collection.Unsubscribe(DataEventType.Add, Process);
		m_OffersManager.Collection.Unsubscribe(DataEventType.Remove, Process);
	}

	protected override async void Preload()
	{
		await Task.WhenAll(
			m_NewsManager.Activate(),
			m_OffersManager.Activate()
		);
		
		base.Preload();
	}

	protected override void Process()
	{
		int value = 0;
		
		value += GetNewsCount();
		value += GetOffersCount();
		
		SetValue(value);
	}

	int GetNewsCount()
	{
		List<string> newsIDs = m_NewsManager.GetNewsIDs();
		
		return newsIDs?.Count(_NewsID => BadgeManager.IsNewsUnread(_NewsID)) ?? 0;
	}

	int GetOffersCount()
	{
		List<string> offerIDs = m_OffersManager.GetAvailableOfferIDs();
		
		return offerIDs?.Count ?? 0;
	}
}
