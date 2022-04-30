using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIMainStorePage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Store;

	[SerializeField] RectTransform m_Container;
	[SerializeField] UIEntity      m_Control;

	[Inject] SignalBus          m_SignalBus;
	[Inject] ProductsProcessor  m_ProductsProcessor;
	[Inject] ProductsDescriptor m_ProductsDescriptor;
	[Inject] UIProductItem.Pool m_ItemPool;

	List<string> m_ProductIDs;

	readonly List<UIProductItem> m_Items = new List<UIProductItem>();

	public void CreateProduct()
	{
		ProductSnapshot snapshot = m_ProductsProcessor.CreateSnapshot();
		
		m_ProductsDescriptor.CreateDescriptor(snapshot.ID);
		
		Refresh();
	}

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
		foreach (UIProductItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		m_ProductIDs = m_ProductsProcessor.GetProductIDs();
		
		if (m_ProductIDs == null || m_ProductIDs.Count == 0)
			return;
		
		foreach (string productID in m_ProductIDs)
		{
			if (string.IsNullOrEmpty(productID))
				continue;
			
			UIProductItem item = m_ItemPool.Spawn(m_Container);
			
			item.Setup(productID);
			
			m_Items.Add(item);
		}
		
		m_Control.BringToFront();
	}
}