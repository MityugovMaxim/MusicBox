using System.Collections.Generic;
using System.Linq;
using Zenject;

public class UINewsBadge : UIBadge
{
	public const string NEWS_GROUP   = "news";
	public const string OFFERS_GROUP = "offers";

	[Inject] NewsManager   m_NewsManager;
	[Inject] OffersManager m_OffersManager;

	protected override void Subscribe()
	{
		m_NewsManager.Collection.Subscribe(DataEventType.Add, Process);
		m_NewsManager.Collection.Subscribe(DataEventType.Remove, Process);
		m_OffersManager.Collection.Subscribe(DataEventType.Add, Process);
		m_OffersManager.Collection.Subscribe(DataEventType.Remove, Process);
	}

	protected override void Unsubscribe()
	{
		m_NewsManager.Collection.Unsubscribe(DataEventType.Add, Process);
		m_NewsManager.Collection.Unsubscribe(DataEventType.Remove, Process);
		m_OffersManager.Collection.Unsubscribe(DataEventType.Add, Process);
		m_OffersManager.Collection.Unsubscribe(DataEventType.Remove, Process);
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
		
		return newsIDs?.Count(_NewsID => BadgeManager.IsUnread(NEWS_GROUP, _NewsID)) ?? 0;
	}

	int GetOffersCount()
	{
		List<string> offerIDs = m_OffersManager.GetAvailableOfferIDs();
		
		return offerIDs?.Count(_OfferID => BadgeManager.IsUnread(OFFERS_GROUP, _OfferID)) ?? 0;
	}
}
