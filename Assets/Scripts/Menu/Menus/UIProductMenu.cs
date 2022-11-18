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
	[SerializeField] UIProductBackground m_Background;
	[SerializeField] UIProductImage      m_Image;
	[SerializeField] UIProductLabel      m_Label;
	[SerializeField] UIProductPrice      m_Price;
	[SerializeField] UIGroup             m_PurchaseGroup;
	[SerializeField] UIGroup             m_LoaderGroup;
	[SerializeField] UIGroup             m_CompleteGroup;

	[SerializeField, Sound] string m_PurchaseSound;

	[Inject] VouchersManager    m_VouchersManager;
	[Inject] StoreProcessor     m_StoreProcessor;
	[Inject] ProductsManager    m_ProductsManager;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] HapticProcessor    m_HapticProcessor;
	[Inject] SoundProcessor     m_SoundProcessor;

	string m_ProductID;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Background.ProductID = m_ProductID;
		m_Image.ProductID      = m_ProductID;
		m_Label.ProductID      = m_ProductID;
		m_Price.ProductID      = m_ProductID;
		
		m_Image.RectTransform.sizeDelta = m_ProductsManager.IsSpecial(m_ProductID)
			? new Vector2(600, 300)
			: new Vector2(350, 350);
		
		Refresh();
	}

	public async void Purchase()
	{
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
			
			await m_MenuProcessor.Hide(MenuType.ProductMenu);
		}
		else
		{
			if (!canceled)
			{
				await m_MenuProcessor.RetryAsync(
					"product_purchase",
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
		
		Refresh();
	}

	protected override bool OnEscape()
	{
		Hide();
		
		return true;
	}

	void Refresh()
	{
		m_Coins.Value = m_VouchersManager.GetProductDiscount(m_ProductID);
		m_Coins.gameObject.SetActive(m_Coins.Value != 0);
		
		m_PurchaseGroup.Show(true);
		m_LoaderGroup.Hide(true);
		m_CompleteGroup.Hide(true);
	}
}
