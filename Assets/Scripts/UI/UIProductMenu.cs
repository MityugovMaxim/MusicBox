using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.ProductMenu)]
public class UIProductMenu : UISlideMenu
{
	[SerializeField] UIUnitLabel         m_CoinsLabel;
	[SerializeField] GameObject          m_ItemsGroup;
	[SerializeField] RectTransform       m_Container;
	[SerializeField] UIProductBackground m_Background;
	[SerializeField] UIProductThumbnail  m_Thumbnail;
	[SerializeField] UIProductLabel      m_Label;
	[SerializeField] UIProductPrice      m_Price;
	[SerializeField] UIGroup             m_PurchaseGroup;
	[SerializeField] UIGroup             m_LoaderGroup;
	[SerializeField] UIGroup             m_CompleteGroup;
	[SerializeField] LevelPreview        m_PreviewSource;

	SignalBus          m_SignalBus;
	ProductProcessor   m_ProductProcessor;
	StoreProcessor     m_StoreProcessor;
	ProfileProcessor   m_ProfileProcessor;
	LevelProcessor     m_LevelProcessor;
	MenuProcessor      m_MenuProcessor;
	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;
	UIProductItem.Pool m_ItemPool;

	string m_ProductID;

	readonly List<UIProductItem> m_Items = new List<UIProductItem>();

	[Inject]
	public void Construct(
		SignalBus          _SignalBus,
		ProductProcessor   _ProductProcessor,
		StoreProcessor     _StoreProcessor,
		ProfileProcessor   _ProfileProcessor,
		LevelProcessor     _LevelProcessor,
		MenuProcessor      _MenuProcessor,
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor,
		UIProductItem.Pool _ItemPool
	)
	{
		m_SignalBus          = _SignalBus;
		m_ProductProcessor   = _ProductProcessor;
		m_StoreProcessor     = _StoreProcessor;
		m_ProfileProcessor   = _ProfileProcessor;
		m_LevelProcessor     = _LevelProcessor;
		m_MenuProcessor      = _MenuProcessor;
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
		m_ItemPool           = _ItemPool;
	}

	public void Setup(string _ProductID)
	{
		Select(_ProductID);
	}

	public async void Purchase()
	{
		m_StatisticProcessor.LogProductMenuPurchaseClick(m_ProductID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		m_PreviewSource.Stop();
		
		foreach (UIProductItem item in m_Items)
			item.Stop();
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await Task.WhenAll(
			m_PurchaseGroup.HideAsync(),
			m_LoaderGroup.ShowAsync()
		);
		
		// TODO: Uncomment
		// bool success = await m_StoreProcessor.Purchase(m_ProductID);
		
		// TODO: Remove
		bool success = true;
		
		#if UNITY_EDITOR
		await Task.Delay(2500);
		#endif
		
		if (success)
		{
			await m_LoaderGroup.HideAsync();
			
			m_HapticProcessor.Process(Haptic.Type.Success);
			
			await m_CompleteGroup.ShowAsync();
			
			await Task.WhenAll(
				m_ProfileProcessor.LoadProfile(),
				Task.Delay(1500)
			);
			
			await m_MenuProcessor.Hide(MenuType.ProductMenu);
		}
		else
		{
			m_HapticProcessor.Process(Haptic.Type.Failure);
			
			await Task.WhenAll(
				m_PurchaseGroup.ShowAsync(),
				m_CompleteGroup.HideAsync(),
				m_LoaderGroup.HideAsync(),
				m_CompleteGroup.HideAsync()
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public void Next()
	{
		m_StatisticProcessor.LogProductMenuNextClick(m_ProductID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		Select(GetProductID(1));
	}

	public void Previous()
	{
		m_StatisticProcessor.LogProductMenuPreviousClick(m_ProductID);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		Select(GetProductID(-1));
	}

	string GetProductID(int _Offset)
	{
		List<string> productIDs = m_ProfileProcessor.GetVisibleProductIDs();
		
		int index = productIDs.IndexOf(m_ProductID);
		if (index >= 0 && index < productIDs.Count)
			return productIDs[MathUtility.Repeat(index + _Offset, productIDs.Count)];
		else if (productIDs.Count > 0)
			return productIDs.FirstOrDefault();
		else
			return m_ProductID;
	}

	protected override void OnShowStarted()
	{
		m_PreviewSource.Stop();
		
		foreach (UIProductItem item in m_Items)
			item.Stop();
		
		Refresh();
		
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProductDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_PreviewSource.Stop();
		
		foreach (UIProductItem item in m_Items)
			item.Stop();
		
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProductDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		foreach (UIProductItem item in m_Items)
		{
			item.Stop();
			m_ItemPool.Despawn(item);
		}
		m_Items.Clear();
		
		List<string> levelIDs = m_ProductProcessor.GetLevelIDs(m_ProductID)
			.Where(m_LevelProcessor.HasLevelID)
			.ToList();
		
		m_ItemsGroup.SetActive(levelIDs.Count > 0);
		
		foreach (string levelID in levelIDs)
		{
			UIProductItem item = m_ItemPool.Spawn();
			
			item.Setup(levelID, PlayPreview, StopPreview);
			
			item.RectTransform.SetParent(m_Container, false);
			
			m_Items.Add(item);
		}
		
		m_Background.Setup(m_ProductID, !Shown);
		m_Thumbnail.Setup(m_ProductID);
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
		
		long coins = m_ProductProcessor.GetCoins(m_ProductID);
		m_CoinsLabel.Value = coins;
		m_CoinsLabel.gameObject.SetActive(coins != 0);
		
		m_PurchaseGroup.Show(true);
		m_LoaderGroup.Hide(true);
		m_CompleteGroup.Hide(true);
		
		m_PreviewSource.Stop();
	}

	void PlayPreview(string _LevelID)
	{
		foreach (UIProductItem item in m_Items)
		{
			if (item.LevelID != _LevelID)
				item.Stop();
		}
		
		m_PreviewSource.Play(_LevelID);
	}

	void StopPreview(string _LevelID)
	{
		m_PreviewSource.Stop();
	}

	void Select(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		Refresh();
	}
}