using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIMainMenuStorePage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Store;

	[SerializeField] RectTransform m_Container;

	[Inject] SignalBus             m_SignalBus;
	[Inject] ProductsManager       m_ProductsManager;
	[Inject] UIProductSpecial.Pool m_SpecialPool;
	[Inject] UIProductPromo.Pool   m_PromoPool;
	[Inject] UIProductGroup.Pool   m_GroupPool;

	readonly List<UIProductSpecial> m_Specials = new List<UIProductSpecial>();
	readonly List<UIProductPromo>   m_Promos   = new List<UIProductPromo>();
	// TODO: Implement
	// readonly List<UIProductDaily>   m_Dailies  = new List<UIProductDaily>();
	readonly List<UIProductGroup>   m_Groups   = new List<UIProductGroup>();

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
		Clear();
		
		CreateSpecial();
		
		CreatePromo();
		
		CreateDaily();
		
		CreateDiscount();
		
		CreateProducts();
	}

	void CreateSpecial()
	{
		List<string> productIDs = m_ProductsManager.GetSpecialProductIDs();
		
		if (productIDs == null || productIDs.Count == 0)
			return;
		
		foreach (string productID in productIDs)
		{
			if (string.IsNullOrEmpty(productID))
				continue;
			
			UIProductSpecial item = m_SpecialPool.Spawn(m_Container);
			
			if (item == null)
				continue;
			
			item.Setup(productID);
			
			m_Specials.Add(item);
		}
	}

	void CreatePromo()
	{
		List<string> productIDs = m_ProductsManager.GetPromoProductIDs();
		
		if (productIDs == null || productIDs.Count == 0)
			return;
		
		foreach (string productID in productIDs)
		{
			if (string.IsNullOrEmpty(productID))
				continue;
			
			UIProductPromo item = m_PromoPool.Spawn(m_Container);
			
			if (item == null)
				continue;
			
			item.Setup(productID);
			
			m_Promos.Add(item);
		}
	}

	void CreateDaily()
	{
		
	}

	void CreateDiscount()
	{
		List<string> productIDs = m_ProductsManager.GetDiscountProductIDs();
		
		if (productIDs == null || productIDs.Count == 0)
			return;
		
		UIProductGroup item = m_GroupPool.Spawn(m_Container);
		
		item.Setup(productIDs);
		
		m_Groups.Add(item);
	}

	void CreateProducts()
	{
		List<string> productIDs = m_ProductsManager.GetAvailableProductIDs();
		
		if (productIDs == null || productIDs.Count == 0)
			return;
		
		UIProductGroup item = m_GroupPool.Spawn(m_Container);
		
		item.Setup(productIDs);
		
		m_Groups.Add(item);
	}

	void Clear()
	{
		foreach (UIProductSpecial item in m_Specials)
			m_SpecialPool.Despawn(item);
		m_Specials.Clear();
		
		foreach (UIProductPromo item in m_Promos)
			m_PromoPool.Despawn(item);
		m_Promos.Clear();
		
		foreach (UIProductGroup item in m_Groups)
			m_GroupPool.Despawn(item);
		m_Groups.Clear();
	}
}