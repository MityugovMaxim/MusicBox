using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIMainMenuNewsPage : UIMainMenuPage
{
	const float LIST_SPACING = 30;

	public override MainMenuPageType Type => MainMenuPageType.News;

	[SerializeField] UILayout m_Content;
	[SerializeField] UIGroup  m_ContentGroup;
	[SerializeField] UIGroup  m_LoaderGroup;

	[Inject] RolesProcessor m_RolesProcessor;
	[Inject] NewsManager    m_NewsManager;
	[Inject] OffersManager  m_OffersManager;

	[Inject] UIAdminElement.Pool m_AdminPool;
	[Inject] UINewsElement.Pool  m_NewsPool;
	[Inject] UIOfferElement.Pool m_OffersPool;

	protected override async void OnShowStarted()
	{
		m_ContentGroup.Hide(true);
		m_LoaderGroup.Show(true);
		
		bool instant = true;
		instant &= await m_NewsManager.Activate();
		instant &= await m_OffersManager.Activate();
		
		if (!IsActive)
			return;
		
		m_ContentGroup.Show(instant);
		m_LoaderGroup.Hide(instant);
		
		Refresh();
		
		m_NewsManager.Collection.Subscribe(DataEventType.Add, Refresh);
		m_NewsManager.Collection.Subscribe(DataEventType.Remove, Refresh);
		m_OffersManager.Collection.Subscribe(DataEventType.Add, Refresh);
		m_OffersManager.Collection.Subscribe(DataEventType.Remove, Refresh);
	}

	protected override void OnHideStarted()
	{
		m_NewsManager.Collection.Subscribe(DataEventType.Add, Refresh);
		m_NewsManager.Collection.Subscribe(DataEventType.Remove, Refresh);
		
		m_OffersManager.Collection.Subscribe(DataEventType.Add, Refresh);
		m_OffersManager.Collection.Subscribe(DataEventType.Remove, Refresh);
	}

	void Refresh()
	{
		m_Content.Clear();
		
		//CreateAdminOffers();
		
		//CreateAdminNews();
		
		//CreateAdminBanners();
		
		CreateAvailableOffers();
		
		CreateNews();
		
		CreateCollectedOffers();
		
		m_Content.Reposition();
	}

	void CreateAdminOffers()
	{
		if (!m_RolesProcessor.HasOffersPermission())
			return;
		
		AdminElementEntity offers = new AdminElementEntity(
			"Edit offers",
			"offers",
			"offers_descriptors",
			typeof(OfferSnapshot),
			m_AdminPool
		);
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(offers);
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateAdminNews()
	{
		if (!m_RolesProcessor.HasNewsPermission())
			return;
		
		AdminElementEntity news = new AdminElementEntity(
			"Edit news",
			"news",
			"news_descriptors",
			typeof(NewsSnapshot),
			m_AdminPool
		);
		
		CreateAdmin(news);
	}

	void CreateAdminBanners()
	{
		if (!m_RolesProcessor.HasBannersPermission())
			return;
		
		AdminElementEntity banners = new AdminElementEntity(
			"Edit banners",
			"banners",
			typeof(BannerSnapshot),
			m_AdminPool
		);
		
		CreateAdmin(banners);
	}

	void CreateAdmin(AdminElementEntity _AdminElement)
	{
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(_AdminElement);
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateAvailableOffers()
	{
		List<string> offerIDs = m_OffersManager.GetAvailableOfferIDs();
		
		if (offerIDs == null || offerIDs.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (string offerID in offerIDs)
			m_Content.Add(new OfferElementEntity(offerID, m_OffersPool));
		
		VerticalStackLayout.End(m_Content);
	}

	void CreateCollectedOffers()
	{
		List<string> offerIDs = m_OffersManager.GetCollectedOfferIDs();
		
		if (offerIDs == null || offerIDs.Count == 0)
			return;
		
		m_Content.Spacing(LIST_SPACING);
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (string offerID in offerIDs)
			m_Content.Add(new OfferElementEntity(offerID, m_OffersPool));
		
		VerticalStackLayout.End(m_Content);
	}

	void CreateNews()
	{
		List<string> newsIDs = m_NewsManager.GetNewsIDs();
		
		if (newsIDs == null || newsIDs.Count == 0)
			return;
		
		m_Content.Spacing(LIST_SPACING);
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (string newsID in newsIDs)
			m_Content.Add(new NewsElementEntity(newsID, m_NewsPool));
		
		VerticalStackLayout.End(m_Content);
	}
}
