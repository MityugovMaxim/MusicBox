using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
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

	[Inject] ProductsProcessor      m_ProductsProcessor;
	[Inject] VouchersProcessor      m_VouchersProcessor;
	[Inject] StoreProcessor         m_StoreProcessor;
	[Inject] ProfileProcessor       m_ProfileProcessor;
	[Inject] ProductsManager        m_ProductsManager;
	[Inject] MenuProcessor          m_MenuProcessor;
	[Inject] HapticProcessor        m_HapticProcessor;
	[Inject] SoundProcessor         m_SoundProcessor;
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
		
		m_Image.RectTransform.sizeDelta = m_ProductsProcessor.IsSpecial(m_ProductID)
			? new Vector2(600, 300)
			: new Vector2(350, 350);
		
		Refresh();
	}

	public async void Purchase()
	{
		m_Preview.Stop();
		
		foreach (UIProductSongItem item in m_Items)
			item.Stop();
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await Task.WhenAll(
			m_PurchaseGroup.HideAsync(),
			m_LoaderGroup.ShowAsync()
		);
		
		bool success  = false;
		bool canceled = false;
		try
		{
			success = await m_StoreProcessor.Purchase(m_ProductID);
		}
		catch (TaskCanceledException)
		{
			canceled = true;
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		
		#if UNITY_EDITOR
		await Task.Delay(1500);
		#endif
		
		if (success)
		{
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
			if (!canceled)
			{
				await m_MenuProcessor.RetryLocalizedAsync(
					"product_purchase",
					"product_menu",
					"PRODUCT_PURCHASE_ERROR_TITLE",
					"COMMON_ERROR_MESSAGE",
					Purchase,
					() => { }
				);
			}
			
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
		Setup(GetProductID(1));
	}

	public void Previous()
	{
		Setup(GetProductID(-1));
	}

	string GetProductID(int _Offset)
	{
		List<string> productIDs = m_ProductsManager.GetProductIDs();
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
		base.OnShowStarted();
		
		m_Preview.Stop();
		
		foreach (UIProductSongItem item in m_Items)
			item.Stop();
		
		Refresh();
	}

	protected override void OnHideFinished()
	{
		base.OnHideFinished();
		
		m_Preview.Stop();
		
		foreach (UIProductSongItem item in m_Items)
			item.Stop();
	}

	protected override bool OnEscape()
	{
		Hide();
		
		return true;
	}

	void Refresh()
	{
		foreach (UIProductSongItem item in m_Items)
		{
			item.Stop();
			m_ItemPool.Despawn(item);
		}
		m_Items.Clear();
		
		List<string> songIDs = m_ProductsProcessor.GetSongIDs(m_ProductID);
		
		m_ItemsGroup.SetActive(songIDs != null && songIDs.Count > 0);
		
		if (songIDs != null && songIDs.Count > 0)
		{
			foreach (string songID in songIDs)
			{
				Debug.LogError(songID);
				
				UIProductSongItem item = m_ItemPool.Spawn(m_Container);
				
				item.Setup(songID, PlayPreview, StopPreview);
				
				m_Items.Add(item);
			}
		}
		
		long coins = m_ProductsProcessor.GetCoins(m_ProductID);
		m_Coins.Value = m_VouchersProcessor.GetValue(VoucherType.ProductDiscount, m_ProductID, coins);
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
