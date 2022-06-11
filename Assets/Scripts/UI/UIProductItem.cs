using System;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIProductItem : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UIProductItem> { }

	static bool Processing { get; set; }

	[SerializeField] UIProductImage    m_Image;
	[SerializeField] UIProductDiscount m_Discount;
	[SerializeField] UIProductPrice    m_Price;
	[SerializeField] UIUnitLabel       m_Coins;
	[SerializeField] UIGroup           m_OverlayGroup;
	[SerializeField] UIGroup           m_LoaderGroup;
	[SerializeField] UIGroup           m_CompleteGroup;

	[SerializeField, Sound] string m_PurchaseSound;

	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] ProfileProcessor   m_ProfileProcessor;
	[Inject] ProductsProcessor  m_ProductsProcessor;
	[Inject] StoreProcessor     m_StoreProcessor;
	[Inject] HapticProcessor    m_HapticProcessor;
	[Inject] SoundProcessor     m_SoundProcessor;

	string m_ProductID;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Image.Setup(m_ProductID);
		m_Discount.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
		
		m_Coins.Value = m_ProductsProcessor.GetCoins(m_ProductID);
		
		m_OverlayGroup.Hide(true);
		m_LoaderGroup.Hide(true);
		m_CompleteGroup.Hide(true);
	}

	protected override void OnClick()
	{
		base.OnClick();
		
		Purchase();
	}

	async void Purchase()
	{
		if (Processing)
			return;
		
		Processing = true;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await Task.WhenAll(
			m_OverlayGroup.ShowAsync(),
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
			
			await m_ProfileProcessor.Load();
			
			await Task.WhenAll(
				m_OverlayGroup.HideAsync(),
				m_CompleteGroup.HideAsync()
			);
		}
		else
		{
			if (!canceled)
			{
				await m_MenuProcessor.RetryLocalizedAsync(
					"product_purchase",
					"PRODUCT_PURCHASE_ERROR_TITLE",
					"PRODUCT_PURCHASE_ERROR_MESSAGE",
					Purchase,
					() => { }
				);
			}
			
			await Task.WhenAll(
				m_OverlayGroup.HideAsync(),
				m_LoaderGroup.HideAsync(),
				m_CompleteGroup.HideAsync()
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		
		Processing = false;
	}
}