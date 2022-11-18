using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIProductItem : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UIProductItem> { }

	[SerializeField] UIProductImage    m_Image;
	[SerializeField] UIProductPrice    m_Price;
	[SerializeField] UIProductTimer    m_Timer;
	[SerializeField] UIProductDiscount m_Discount;
	[SerializeField] UIProductCoins    m_Coins;
	[SerializeField] UIGroup           m_OverlayGroup;
	[SerializeField] UIGroup           m_LoaderGroup;
	[SerializeField] UIGroup           m_CompleteGroup;

	[SerializeField, Sound] string m_PurchaseSound;

	[Inject] ProductsManager m_ProductsManager;
	[Inject] MenuProcessor   m_MenuProcessor;
	[Inject] HapticProcessor m_HapticProcessor;
	[Inject] SoundProcessor  m_SoundProcessor;

	string m_ProductID;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Image.ProductID    = m_ProductID;
		m_Price.ProductID    = m_ProductID;
		m_Timer.ProductID    = m_ProductID;
		m_Discount.ProductID = m_ProductID;
		m_Coins.ProductID    = m_ProductID;
		
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
		await Task.WhenAll(
			m_OverlayGroup.ShowAsync(),
			m_LoaderGroup.ShowAsync()
		);
		
		try
		{
			await m_ProductsManager.Purchase(m_ProductID);
		}
		catch (TaskCanceledException)
		{
			await Task.WhenAll(
				m_OverlayGroup.HideAsync(),
				m_LoaderGroup.HideAsync(),
				m_CompleteGroup.HideAsync()
			);
			
			return;
		}
		catch (Exception)
		{
			await Task.WhenAll(
				m_OverlayGroup.HideAsync(),
				m_LoaderGroup.HideAsync(),
				m_CompleteGroup.HideAsync()
			);
			
			await m_MenuProcessor.ErrorAsync("purchase");
			
			return;
		}
		
		await m_LoaderGroup.HideAsync();
		
		m_HapticProcessor.Process(Haptic.Type.Success);
		m_SoundProcessor.Play(m_PurchaseSound);
		
		await m_CompleteGroup.ShowAsync();
		
		await Task.WhenAll(
			m_OverlayGroup.HideAsync(),
			m_CompleteGroup.HideAsync()
		);
	}
}
