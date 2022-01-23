using System.Collections;
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

	MainMenuPageType m_PageType;
	SignalBus        m_SignalBus;
	SocialProcessor  m_SocialProcessor;
	ProfileProcessor m_ProfileProcessor;
	NewsProcessor    m_NewsProcessor;

	IEnumerator m_MoveRoutine;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_SignalBus.Unsubscribe<SocialDataUpdateSignal>(Process);
		m_SignalBus.Unsubscribe<OfferDataUpdateSignal>(ProcessOffersBadge);
		m_SignalBus.Unsubscribe<NewsDataUpdateSignal>(ProcessNewsBadge);
		m_SignalBus.Unsubscribe<ProductDataUpdateSignal>(ProcessStoreBadge);
	}

	[Inject]
	public void Construct(
		SignalBus        _SignalBus,
		SocialProcessor  _SocialProcessor,
		ProfileProcessor _ProfileProcessor,
		NewsProcessor    _NewsProcessor
	)
	{
		m_SignalBus        = _SignalBus;
		m_SocialProcessor  = _SocialProcessor;
		m_ProfileProcessor = _ProfileProcessor;
		m_NewsProcessor    = _NewsProcessor;
		
		m_SignalBus.Subscribe<SocialDataUpdateSignal>(Process);
		m_SignalBus.Subscribe<OfferDataUpdateSignal>(ProcessOffersBadge);
		m_SignalBus.Subscribe<NewsDataUpdateSignal>(ProcessNewsBadge);
		m_SignalBus.Subscribe<ProductDataUpdateSignal>(ProcessStoreBadge);
	}

	public void Select(MainMenuPageType _PageType, bool _Instant = false)
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
		
		PlayerPrefs.Save();
	}

	void ProcessOffersBadge()
	{
		if (m_PageType == MainMenuPageType.Offers)
		{
			ReadOffers();
			return;
		}
		
		List<string> offerIDs = m_ProfileProcessor.GetVisibleOfferIDs();
		
		m_OffersBadge.Value = GetUnreadCount(OFFERS_KEY, offerIDs);
	}

	void ProcessNewsBadge()
	{
		if (m_PageType == MainMenuPageType.News)
		{
			ReadNews();
			return;
		}
		
		m_NewsBadge.Value = GetUnreadCount(NEWS_KEY, m_NewsProcessor.GetNewsIDs());
	}

	void ProcessStoreBadge()
	{
		if (m_PageType == MainMenuPageType.Store)
		{
			ReadStore();
			return;
		}
		
		List<string> productIDs = m_ProfileProcessor.GetVisibleProductIDs();
		
		m_StoreBadge.Value = GetUnreadCount(STORE_KEY, productIDs);
	}

	void ReadOffers()
	{
		Read(OFFERS_KEY, m_ProfileProcessor.GetVisibleOfferIDs());
		
		m_OffersBadge.Value = 0;
	}

	void ReadNews()
	{
		Read(NEWS_KEY, m_NewsProcessor.GetNewsIDs());
		
		m_NewsBadge.Value = 0;
	}

	void ReadStore()
	{
		Read(STORE_KEY, m_ProfileProcessor.GetVisibleProductIDs());
		
		m_StoreBadge.Value = 0;
	}

	int GetUnreadCount(string _Key, IEnumerable<string> _IDs)
	{
		string userID = m_SocialProcessor.UserID;
		int count = 0;
		foreach (string id in _IDs)
		{
			if (!PlayerPrefs.HasKey($"{_Key}_{userID}_{id}"))
				count++;
		}
		return count;
	}

	void Read(string _Key, IEnumerable<string> _IDs)
	{
		string userID = m_SocialProcessor.UserID;
		foreach (string id in _IDs)
			PlayerPrefs.SetInt($"{_Key}_{userID}_{id}", 1);
	}
}