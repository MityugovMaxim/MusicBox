using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.ProductMenu)]
public class UIProductMenu : UISlideMenu
{
	[SerializeField] UIUnitLabel         m_Coins;
	[SerializeField] RectTransform       m_Container;
	[SerializeField] UIProductBackground m_Background;
	[SerializeField] UIProductImage      m_Image;
	[SerializeField] UIProductLabel      m_Label;
	[SerializeField] UIProductPrice      m_Price;
	[SerializeField] GameObject          m_ItemsGroup;
	[SerializeField] UIGroup             m_PurchaseGroup;
	[SerializeField] UIGroup             m_LoaderGroup;
	[SerializeField] UIGroup             m_CompleteGroup;
	[SerializeField] SongPreview         m_Preview;

	[SerializeField, Sound] string m_PurchaseSound;

	[Inject] SignalBus              m_SignalBus;
	[Inject] ProductsProcessor      m_ProductsProcessor;
	[Inject] StoreProcessor         m_StoreProcessor;
	[Inject] ProfileProcessor       m_ProfileProcessor;
	[Inject] SongsProcessor         m_SongsProcessor;
	[Inject] MenuProcessor          m_MenuProcessor;
	[Inject] HapticProcessor        m_HapticProcessor;
	[Inject] SoundProcessor         m_SoundProcessor;
	[Inject] StatisticProcessor     m_StatisticProcessor;
	[Inject] UIProductSongItem.Pool m_ItemPool;

	string m_ProductID;

	readonly List<UIProductSongItem> m_Items = new List<UIProductSongItem>();

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Background.Setup(m_ProductID);
		m_Image.Setup(m_ProductID);
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
		
		Refresh();
	}

	public async void Purchase()
	{
		m_StatisticProcessor.LogProductMenuPurchaseClick(m_ProductID);
		
		m_Preview.Stop();
		
		foreach (UIProductSongItem item in m_Items)
			item.Stop();
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await Task.WhenAll(
			m_PurchaseGroup.HideAsync(),
			m_LoaderGroup.ShowAsync()
		);
		
		bool success = await m_StoreProcessor.Purchase(m_ProductID);
		
		#if UNITY_EDITOR
		await Task.Delay(1500);
		#endif
		
		if (success)
		{
			m_StatisticProcessor.LogProductMenuPurchaseSuccess(m_ProductID);
			
			await m_LoaderGroup.HideAsync();
			
			m_HapticProcessor.Process(Haptic.Type.Success);
			m_SoundProcessor.Play(m_PurchaseSound);
			
			await m_CompleteGroup.ShowAsync();
			
			await Task.WhenAll(
				m_ProfileProcessor.Load(),
				Task.Delay(1500)
			);
			
			await m_MenuProcessor.Hide(MenuType.ProductMenu);
		}
		else
		{
			m_StatisticProcessor.LogProductMenuPurchaseFailed(m_ProductID);
			
			await m_MenuProcessor.RetryLocalizedAsync(
				"product_purchase",
				"PRODUCT_PURCHASE_ERROR_TITLE",
				"PRODUCT_PURCHASE_ERROR_MESSAGE",
				Purchase,
				() => { }
			);
			
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
		
		Setup(GetProductID(1));
	}

	public void Previous()
	{
		m_StatisticProcessor.LogProductMenuPreviousClick(m_ProductID);
		
		Setup(GetProductID(-1));
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
		m_Preview.Stop();
		
		foreach (UIProductSongItem item in m_Items)
			item.Stop();
		
		Refresh();
		
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProductsDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProductsDataUpdateSignal>(Refresh);
	}

	protected override void OnHideFinished()
	{
		m_Preview.Stop();
		
		foreach (UIProductSongItem item in m_Items)
			item.Stop();
	}

	void Refresh()
	{
		foreach (UIProductSongItem item in m_Items)
		{
			item.Stop();
			m_ItemPool.Despawn(item);
		}
		m_Items.Clear();
		
		List<string> songIDs = m_ProductsProcessor.GetSongIDs(m_ProductID)
			.Where(_SongID => m_SongsProcessor.GetMode(_SongID) == SongMode.Product)
			.ToList();
		
		m_ItemsGroup.SetActive(songIDs.Count > 0);
		
		foreach (string levelID in songIDs)
		{
			UIProductSongItem item = m_ItemPool.Spawn();
			
			item.Setup(levelID, PlayPreview, StopPreview);
			
			item.RectTransform.SetParent(m_Container, false);
			
			m_Items.Add(item);
		}
		
		long coins = m_ProductsProcessor.GetCoins(m_ProductID);
		m_Coins.Value = coins;
		m_Coins.gameObject.SetActive(coins != 0);
		
		m_PurchaseGroup.Show(true);
		m_LoaderGroup.Hide(true);
		m_CompleteGroup.Hide(true);
		
		m_Preview.Stop();
	}

	void PlayPreview(string _SongID)
	{
		foreach (UIProductSongItem item in m_Items)
		{
			if (item.SongID != _SongID)
				item.Stop();
		}
		
		m_Preview.Play(_SongID);
	}

	void StopPreview(string _SongID)
	{
		m_Preview.Stop();
	}
}