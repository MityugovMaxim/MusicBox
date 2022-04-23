using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIMainMenuControl : UIEntity
{
	const string NEWS_KEY   = "NEWS_BADGE";
	const string OFFERS_KEY = "OFFERS_BADGE";
	const string STORE_KEY  = "STORE_BADGE";

	[SerializeField] UIMainMenuButton[] m_Buttons;
	[SerializeField] UIMainMenuRing     m_Ring;
	[SerializeField] UIBadge            m_OffersBadge;
	[SerializeField] UIBadge            m_NewsBadge;
	[SerializeField] UIBadge            m_StoreBadge;

	[Inject] SignalBus        m_SignalBus;
	[Inject] OffersManager    m_OffersManager;
	[Inject] SocialProcessor  m_SocialProcessor;
	[Inject] ProfileProcessor m_ProfileProcessor;
	[Inject] NewsProcessor    m_NewsProcessor;

	readonly HashSet<string> m_NewsIDs    = new HashSet<string>();
	readonly HashSet<string> m_OfferIDs   = new HashSet<string>();
	readonly HashSet<string> m_ProductIDs = new HashSet<string>();

	MainMenuPageType m_PageType;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		if (m_SignalBus == null)
			return;
		
		m_SignalBus.Subscribe<SocialDataUpdateSignal>(Process);
		m_SignalBus.Subscribe<OffersDataUpdateSignal>(ProcessOffersBadge);
		m_SignalBus.Subscribe<NewsDataUpdateSignal>(ProcessNewsBadge);
		m_SignalBus.Subscribe<ProductsDataUpdateSignal>(ProcessStoreBadge);
	}

	protected override void OnDisable()
	{
		base.OnDestroy();
		
		if (m_SignalBus == null)
			return;
		
		m_SignalBus.Unsubscribe<SocialDataUpdateSignal>(Process);
		m_SignalBus.Unsubscribe<OffersDataUpdateSignal>(ProcessOffersBadge);
		m_SignalBus.Unsubscribe<NewsDataUpdateSignal>(ProcessNewsBadge);
		m_SignalBus.Unsubscribe<ProductsDataUpdateSignal>(ProcessStoreBadge);
	}

	public void Select(MainMenuPageType _PageType, bool _Instant)
	{
		m_PageType = _PageType;
		
		Read();
		
		foreach (UIMainMenuButton button in m_Buttons)
			button.Toggle(button.PageType == m_PageType, _Instant);
		
		foreach (UIMainMenuButton button in m_Buttons)
		{
			if (button.PageType != m_PageType)
				continue;
			
			m_Ring.Move(button.RectTransform, _Instant);
			
			break;
		}
	}

	void Process()
	{
		if (m_PageType != MainMenuPageType.Offers)
			ProcessOffersBadge();
		if (m_PageType != MainMenuPageType.News)
			ProcessNewsBadge();
		if (m_PageType != MainMenuPageType.Store)
			ProcessStoreBadge();
	}

	void Read()
	{
		switch (m_PageType)
		{
			case MainMenuPageType.Offers:
				ReadOffers();
				break;
			case MainMenuPageType.News:
				ReadNews();
				break;
			case MainMenuPageType.Store:
				ReadStore();
				break;
		}
	}

	void ProcessOffersBadge()
	{
		if (m_PageType == MainMenuPageType.Offers)
		{
			ReadOffers();
			return;
		}
		
		List<string> offerIDs = m_OffersManager.GetAvailableOfferIDs();
		
		m_OffersBadge.Value = GetUnreadCount(OFFERS_KEY, offerIDs, m_OfferIDs);
	}

	void ProcessNewsBadge()
	{
		if (m_PageType == MainMenuPageType.News)
		{
			ReadNews();
			return;
		}
		
		m_NewsBadge.Value = GetUnreadCount(NEWS_KEY, m_NewsProcessor.GetNewsIDs(), m_NewsIDs);
	}

	void ProcessStoreBadge()
	{
		if (m_PageType == MainMenuPageType.Store)
		{
			ReadStore();
			return;
		}
		
		List<string> productIDs = m_ProfileProcessor.GetVisibleProductIDs();
		
		m_StoreBadge.Value = GetUnreadCount(STORE_KEY, productIDs, m_ProductIDs);
	}

	void ReadOffers()
	{
		Read(OFFERS_KEY, m_OffersManager.GetAvailableOfferIDs(), m_OfferIDs);
		
		m_OffersBadge.Value = 0;
	}

	void ReadNews()
	{
		Read(NEWS_KEY, m_NewsProcessor.GetNewsIDs(), m_NewsIDs);
		
		m_NewsBadge.Value = 0;
	}

	void ReadStore()
	{
		Read(STORE_KEY, m_ProfileProcessor.GetVisibleProductIDs(), m_ProductIDs);
		
		m_StoreBadge.Value = 0;
	}

	int GetUnreadCount(string _Key, IEnumerable<string> _IDs, HashSet<string> _CachedIDs)
	{
		string userID = m_SocialProcessor.UserID;
		int count = 0;
		foreach (string id in _IDs)
		{
			if (!PlayerPrefs.HasKey($"{_Key}_{userID}_{id}") && !_CachedIDs.Contains(id))
				count++;
		}
		return count;
	}

	void Read(string _Key, IEnumerable<string> _IDs, HashSet<string> _CachedIDs)
	{
		string userID = m_SocialProcessor.UserID;
		foreach (string id in _IDs)
		{
			PlayerPrefs.SetInt($"{_Key}_{userID}_{id}", 1);
			_CachedIDs.Add(id);
		}
	}
}