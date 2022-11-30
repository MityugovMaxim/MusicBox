using System.Threading.Tasks;
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

	[Inject] VouchersManager     m_VouchersManager;
	[Inject] ProductsManager m_ProductsManager;
	[Inject] MenuProcessor       m_MenuProcessor;
	[Inject] HapticProcessor     m_HapticProcessor;
	[Inject] SoundProcessor      m_SoundProcessor;

	string m_ProductID;

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Background.ProductID = m_ProductID;
		m_Image.ProductID      = m_ProductID;
		m_Label.ProductID      = m_ProductID;
		m_Price.ProductID      = m_ProductID;
		
		Refresh();
	}

	public async void Purchase()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await Task.WhenAll(
			m_PurchaseGroup.HideAsync(),
			m_LoaderGroup.ShowAsync()
		);
		
		RequestState state = await m_ProductsManager.Purchase(m_ProductID);
		
		#if UNITY_EDITOR
		await Task.Delay(1500);
		#endif
		
		if (state == RequestState.Success)
		{
			await m_LoaderGroup.HideAsync();
			
			m_HapticProcessor.Process(Haptic.Type.Success);
			m_SoundProcessor.Play(m_PurchaseSound);
			
			await m_CompleteGroup.ShowAsync();
			
			await m_MenuProcessor.Hide(MenuType.ProductMenu);
		}
		else if (state == RequestState.Fail)
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
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
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
