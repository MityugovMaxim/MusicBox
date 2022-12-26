using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIMainMenuStorePage : UIMainMenuPage
{
	const float GRID_SPACING = 30;
	const float LIST_SPACING = 30;
	const float ITEM_ASPECT  = 0.675f;

	public override MainMenuPageType Type => MainMenuPageType.Store;

	[SerializeField] UILayout m_Content;
	[SerializeField] UIGroup  m_ContentGroup;
	[SerializeField] UIGroup  m_LoaderGroup;

	[Inject] ProductsManager m_ProductsManager;
	[Inject] TimersManager   m_TimersManager;
	[Inject] DailyManager    m_DailyManager;

	[Inject] RolesProcessor              m_RolesProcessor;
	[Inject] UIAdminElement.Pool         m_AdminPool;
	[Inject] UIDailyElement.Pool         m_DailyPool;
	[Inject] UIProductCoinsElement.Pool  m_CoinsPool;
	[Inject] UIProductSeasonElement.Pool m_SeasonsPool;

	protected override async void OnShowStarted()
	{
		m_ContentGroup.Hide(true);
		m_LoaderGroup.Show(true);
		
		int frame = Time.frameCount;
		
		await Task.WhenAll(
			m_TimersManager.Activate(),
			m_DailyManager.Activate()
		);
		
		bool instant = frame == Time.frameCount;
		
		m_LoaderGroup.Hide(instant);
		m_ContentGroup.Show(instant);
		
		Refresh();
		
		m_ProductsManager.Profile.Subscribe(DataEventType.Add, Refresh);
		m_ProductsManager.Profile.Subscribe(DataEventType.Remove, Refresh);
	}

	protected override void OnHideStarted()
	{
		m_ProductsManager.Profile.Unsubscribe(DataEventType.Add, Refresh);
		m_ProductsManager.Profile.Unsubscribe(DataEventType.Remove, Refresh);
	}

	void Refresh()
	{
		m_Content.Clear();
		
		if (AdminMode.Enabled)
		{
			CreateAdminProducts();
			CreateAdminDaily();
			CreateAdminAds();
		}
		
		CreateSeasons();
		
		CreateDaily();
		
		CreateCoins();
		
		m_Content.Reposition();
	}

	void CreateAdminProducts()
	{
		if (!m_RolesProcessor.HasProductsPermission())
			return;
		
		AdminElementEntity products = new AdminElementEntity(
			"Edit products",
			"products",
			"products_descriptors",
			typeof(ProductSnapshot),
			m_AdminPool
		);
		
		CreateAdmin(products);
	}

	void CreateAdminDaily()
	{
		if (!m_RolesProcessor.HasDailyPermission())
			return;
		
		AdminElementEntity daily = new AdminElementEntity(
			"Edit daily",
			"daily",
			typeof(DailySnapshot),
			m_AdminPool
		);
		
		CreateAdmin(daily);
	}

	void CreateAdminAds()
	{
		if (!m_RolesProcessor.HasAdsPermission())
			return;
		
		AdminElementEntity ads = new AdminElementEntity(
			"Edit ads",
			"ads_providers",
			typeof(AdsProviderSnapshot),
			m_AdminPool
		);
		
		CreateAdmin(ads);
	}

	void CreateAdmin(AdminElementEntity _AdminElement)
	{
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(_AdminElement);
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateSeasons()
	{
		List<string> productIDs = m_ProductsManager.GetProductIDs(ProductType.Season);
		
		if (productIDs == null || productIDs.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (string productID in productIDs)
			m_Content.Add(new ProductSeasonElementEntity(productID, m_SeasonsPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateDaily()
	{
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(new DailyElementEntity(m_DailyPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateCoins()
	{
		List<string> productIDs = m_ProductsManager.GetProductIDs(ProductType.Coins);
		
		if (productIDs == null || productIDs.Count == 0)
			return;
		
		VerticalGridLayout.Start(m_Content, 3, ITEM_ASPECT, GRID_SPACING / 2, GRID_SPACING);
		
		foreach (string productID in productIDs)
			m_Content.Add(new ProductCoinsElementEntity(productID, m_CoinsPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}
}
