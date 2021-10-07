using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIStorePage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Store;

	[SerializeField] RectTransform m_Container;
	[SerializeField] UILoader      m_Loader;
	[SerializeField] UIGroup       m_ItemsGroup;
	[SerializeField] UIGroup       m_LoaderGroup;
	[SerializeField] UIGroup       m_ErrorGroup;

	SignalBus           m_SignalBus;
	StoreProcessor      m_StoreProcessor;
	UIStorePageItem.Pool m_ItemPool;

	List<string> m_ProductIDs;

	readonly List<UIStorePageItem> m_Items = new List<UIStorePageItem>();

	[Inject]
	public void Construct(
		SignalBus           _SignalBus,
		StoreProcessor      _StoreProcessor,
		UIStorePageItem.Pool _ItemPool
	)
	{
		m_SignalBus      = _SignalBus;
		m_StoreProcessor = _StoreProcessor;
		m_ItemPool       = _ItemPool;
	}

	public async void Reload(bool _Instant = false)
	{
		if (m_StoreProcessor.Loaded)
		{
			Refresh();
			m_LoaderGroup.Hide(true);
			m_ErrorGroup.Hide(true);
			m_ItemsGroup.Show(true);
			return;
		}
		
		m_ItemsGroup.Hide(_Instant);
		m_ErrorGroup.Hide(_Instant);
		m_LoaderGroup.Show(_Instant);
		
		m_Loader.Restore();
		m_Loader.Play();
		
		await Task.Delay(750);
		
		bool initialized = await m_StoreProcessor.LoadStore();
		
		if (initialized)
		{
			Refresh();
			m_LoaderGroup.Hide();
			m_ErrorGroup.Hide();
			m_ItemsGroup.Show();
		}
		else
		{
			m_LoaderGroup.Hide();
			m_ItemsGroup.Hide();
			m_ErrorGroup.Show();
		}
	}

	protected override async void OnShowStarted()
	{
		Reload(true);
		
		m_SignalBus.Subscribe<PurchaseSignal>(Refresh);
	}

	protected override void OnHideFinished()
	{
		m_SignalBus.Unsubscribe<PurchaseSignal>(Refresh);
	}

	void Refresh()
	{
		foreach (UIStorePageItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		m_ProductIDs = m_StoreProcessor.GetProductIDs();
		
		if (m_ProductIDs == null || m_ProductIDs.Count == 0)
			return;
		
		foreach (string productID in m_ProductIDs)
		{
			UIStorePageItem item = m_ItemPool.Spawn();
			
			item.Setup(productID);
			
			item.RectTransform.SetParent(m_Container, false);
			
			m_Items.Add(item);
		}
	}
}