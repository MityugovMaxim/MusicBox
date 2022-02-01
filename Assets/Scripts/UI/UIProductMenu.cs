using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.ProductMenu)]
public class UIProductMenu : UISlideMenu
{
	[SerializeField] UIProductMenuItem       m_Item;
	[SerializeField] GameObject              m_ItemsGroup;
	[SerializeField] RectTransform           m_Container;
	[SerializeField] UIProductBackground     m_Background;
	[SerializeField] UIProductThumbnail      m_Thumbnail;
	[SerializeField] UIProductLabel          m_Label;
	[SerializeField] UIProductPrice          m_Price;
	[SerializeField] UIGroup                 m_PurchaseGroup;
	[SerializeField] UIGroup                 m_LoaderGroup;
	[SerializeField] UIGroup                 m_SuccessGroup;
	[SerializeField] UILoader                m_Loader;
	[SerializeField] LevelPreviewAudioSource m_PreviewSource;

	SignalBus                 m_SignalBus;
	ProductProcessor          m_ProductProcessor;
	StoreProcessor            m_StoreProcessor;
	ProfileProcessor          m_ProfileProcessor;
	LevelProcessor            m_LevelProcessor;
	MenuProcessor             m_MenuProcessor;
	HapticProcessor           m_HapticProcessor;
	UIProductMenuItem.Factory m_ItemFactory;

	string m_ProductID;

	readonly List<UIProductMenuItem> m_Items = new List<UIProductMenuItem>();

	[Inject]
	public void Construct(
		SignalBus                 _SignalBus,
		ProductProcessor          _ProductProcessor,
		StoreProcessor            _StoreProcessor,
		ProfileProcessor          _ProfileProcessor,
		LevelProcessor            _LevelProcessor,
		MenuProcessor             _MenuProcessor,
		HapticProcessor           _HapticProcessor,
		UIProductMenuItem.Factory _ItemFactory
	)
	{
		m_SignalBus        = _SignalBus;
		m_ProductProcessor = _ProductProcessor;
		m_StoreProcessor   = _StoreProcessor;
		m_ProfileProcessor = _ProfileProcessor;
		m_LevelProcessor   = _LevelProcessor;
		m_MenuProcessor    = _MenuProcessor;
		m_HapticProcessor  = _HapticProcessor;
		m_ItemFactory      = _ItemFactory;
	}

	public void Setup(string _ProductID)
	{
		Select(_ProductID);
	}

	public void Next()
	{
		Select(GetProductID(1));
	}

	public void Previous()
	{
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

	public async void Purchase()
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		m_PreviewSource.Stop();
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		m_PurchaseGroup.Hide();
		m_LoaderGroup.Show();
		m_SuccessGroup.Hide();
		
		m_Loader.Restore();
		
		#if UNITY_EDITOR
		await Task.Delay(5000);
		#endif
		
		bool success = await m_StoreProcessor.Purchase(m_ProductID);
		
		if (success)
		{
			m_HapticProcessor.Process(Haptic.Type.Success);
			
			await Task.WhenAll(
				m_PurchaseGroup.HideAsync(),
				m_LoaderGroup.HideAsync(),
				m_SuccessGroup.ShowAsync()
			);
			
			await m_ProfileProcessor.LoadProfile();
			
			await m_MenuProcessor.Hide(MenuType.ProductMenu);
		}
		else
		{
			m_HapticProcessor.Process(Haptic.Type.Failure);
			
			await Task.WhenAll(
				m_PurchaseGroup.ShowAsync(),
				m_LoaderGroup.HideAsync(),
				m_SuccessGroup.HideAsync()
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	protected override void OnShowStarted()
	{
		m_PreviewSource.Stop();
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
		
		Refresh();
		
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProductDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_PreviewSource.Stop();
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
		
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProductDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		string[] levelIDs = m_ProductProcessor.GetLevelIDs(m_ProductID)
			.Where(m_LevelProcessor.Contains)
			.ToArray();
		
		if (levelIDs.Length == 0)
		{
			m_ItemsGroup.SetActive(false);
			return;
		}
		
		m_ItemsGroup.SetActive(true);
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
		
		int delta = levelIDs.Length - m_Items.Count;
		int count = Mathf.Abs(delta);
		
		if (delta > 0)
		{
			for (int i = 0; i < count; i++)
			{
				UIProductMenuItem item = m_ItemFactory.Create(m_Item);
				item.RectTransform.SetParent(m_Container, false);
				m_Items.Add(item);
			}
		}
		else if (delta < 0)
		{
			for (int i = 0; i < count; i++)
			{
				int               index = m_Items.Count - 1;
				UIProductMenuItem item  = m_Items[index];
				Destroy(item.gameObject);
				m_Items.RemoveAt(index);
			}
		}
		
		foreach (UIProductMenuItem item in m_Items)
			item.gameObject.SetActive(false);
		
		for (var i = 0; i < levelIDs.Length; i++)
		{
			UIProductMenuItem item    = m_Items[i];
			string            levelID = levelIDs[i];
			
			item.Setup(levelID, PlayPreview, StopPreview);
			
			item.gameObject.SetActive(true);
		}
	}

	void PlayPreview(string _LevelID)
	{
		foreach (UIProductMenuItem item in m_Items)
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
		
		m_Background.Setup(m_ProductID, !Shown);
		m_Thumbnail.Setup(m_ProductID);
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
		
		m_PurchaseGroup.Show(true);
		m_LoaderGroup.Hide(true);
		m_SuccessGroup.Hide(true);
		
		m_PreviewSource.Stop();
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
	}
}