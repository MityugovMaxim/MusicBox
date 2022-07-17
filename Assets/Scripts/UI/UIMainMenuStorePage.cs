using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIMainMenuStorePage : UIMainMenuPage
{
	const float GRID_SPACING = 30;
	const float LIST_SPACING = 30;
	const float ITEM_ASPECT  = 0.675f;

	public override MainMenuPageType Type => MainMenuPageType.Store;

	[SerializeField] UILayout m_Content;

	[Inject] SignalBus             m_SignalBus;
	[Inject] DailyManager          m_DailyManager;
	[Inject] ProductsManager       m_ProductsManager;
	[Inject] RolesProcessor        m_RolesProcessor;
	[Inject] UIAdminElement.Pool   m_AdminPool;
	[Inject] UIDailyElement.Pool   m_DailyPool;
	[Inject] UIProductSpecial.Pool m_SpecialPool;
	[Inject] UIProductPromo.Pool   m_PromoPool;
	[Inject] UIProductItem.Pool    m_ItemPool;

	protected override void OnShowStarted()
	{
		Refresh();
		
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProductsDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProductsDataUpdateSignal>(Refresh);
	}

	async void Refresh()
	{
		await UnityTask.While(() => UIProductItem.Processing);
		
		m_Content.Clear();
		
		CreateAdminProducts();
		
		CreateAdminDaily();
		
		CreateSpecial();
		
		CreatePromo();
		
		CreateDaily();
		
		CreateItems();
		
		m_Content.Reposition();
	}

	void CreateAdminProducts()
	{
		if (!m_RolesProcessor.HasProductsPermission())
			return;
		
		AdminElementEntity products = new AdminElementEntity(
			"Edit products",
			"products",
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

	void CreateAdmin(AdminElementEntity _AdminElement)
	{
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(_AdminElement);
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateDaily()
	{
		List<string> dailyIDs = m_DailyManager.GetDailyIDs();
		
		if (dailyIDs == null || dailyIDs.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		m_Content.Add(new DailyElementEntity(m_DailyPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateSpecial()
	{
		List<string> productIDs = m_ProductsManager.GetSpecialProductIDs();
		
		if (productIDs == null || productIDs.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (string productID in productIDs)
			m_Content.Add(new ProductSpecialEntity(productID, m_SpecialPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreatePromo()
	{
		List<string> productIDs = m_ProductsManager.GetPromoProductIDs();
		
		if (productIDs == null || productIDs.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (string productID in productIDs)
			m_Content.Add(new ProductPromoEntity(productID, m_PromoPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}

	void CreateItems()
	{
		List<string> productIDs = m_ProductsManager.GetAvailableProductIDs();
		
		if (productIDs == null || productIDs.Count == 0)
			return;
		
		VerticalGridLayout.Start(m_Content, 3, ITEM_ASPECT, GRID_SPACING / 2, GRID_SPACING);
		
		foreach (string productID in productIDs)
			m_Content.Add(new ProductItemEntity(productID, m_ItemPool));
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
	}
}