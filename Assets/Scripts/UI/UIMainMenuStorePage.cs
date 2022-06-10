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
	[Inject] ProductsManager       m_ProductsManager;
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

	void Refresh()
	{
		m_Content.Clear();
		
		CreateSpecial();
		
		CreatePromo();
		
		CreateItems();
		
		m_Content.Reposition();
	}

	void CreateSpecial()
	{
		List<string> productIDs = m_ProductsManager.GetSpecialProductIDs();
		
		if (productIDs == null || productIDs.Count == 0)
			return;
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (string productID in productIDs)
			m_Content.Add(new ProductSpecialEntity(productID, m_SpecialPool));
		
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
		
		m_Content.Space(LIST_SPACING);
	}
}