using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIMainMenuNewsPage : UIMainMenuPage
{
	const float LIST_SPACING = 30;

	public override MainMenuPageType Type => MainMenuPageType.News;

	[SerializeField] UILayout m_Content;

	[Inject] SignalBus           m_SignalBus;
	[Inject] RolesProcessor      m_RolesProcessor;
	[Inject] NewsProcessor       m_NewsProcessor;
	[Inject] UIAdminElement.Pool m_AdminPool;
	[Inject] UINewsItem.Pool     m_ItemPool;

	protected override void OnShowStarted()
	{
		Refresh();
		
		m_SignalBus.Subscribe<NewsDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<NewsDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		m_Content.Clear();
		
		CreateAdminNews();
		
		CreateAdminBanners();
		
		CreateNews();
		
		m_Content.Reposition();
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

	void CreateNews()
	{
		List<string> newsIDs = m_NewsProcessor.GetNewsIDs();
		
		if (newsIDs == null || newsIDs.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (string newsID in newsIDs)
			m_Content.Add(new NewsItemEntity(newsID, m_ItemPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}
}
