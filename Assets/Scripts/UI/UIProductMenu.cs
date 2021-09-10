using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

[Menu(MenuType.ProductMenu)]
public class UIProductMenu : UISlideMenu, IInitializable, IDisposable
{
	[SerializeField] UIProductMenuItem          m_Item;
	[SerializeField] GameObject                 m_ItemsGroup;
	[SerializeField] RectTransform              m_Container;
	[SerializeField] UIProductPreviewBackground m_Background;
	[SerializeField] UIProductPreviewThumbnail  m_Thumbnail;
	[SerializeField] UIProductPreviewLabel      m_Label;
	[SerializeField] UIProductPreviewPrice      m_Price;
	[SerializeField] LevelPreviewAudioSource    m_PreviewSource;

	SignalBus                 m_SignalBus;
	PurchaseProcessor         m_PurchaseProcessor;
	LevelProcessor            m_LevelProcessor;
	MenuProcessor             m_MenuProcessor;
	HapticProcessor           m_HapticProcessor;
	UIProductMenuItem.Factory m_ItemFactory;

	string m_ProductID;

	readonly List<UIProductMenuItem> m_Items = new List<UIProductMenuItem>();

	[Inject]
	public void Construct(
		SignalBus                 _SignalBus,
		PurchaseProcessor         _PurchaseProcessor,
		LevelProcessor            _LevelProcessor,
		MenuProcessor             _MenuProcessor,
		HapticProcessor           _HapticProcessor,
		UIProductMenuItem.Factory _ItemFactory
	)
	{
		m_SignalBus         = _SignalBus;
		m_PurchaseProcessor = _PurchaseProcessor;
		m_LevelProcessor    = _LevelProcessor;
		m_MenuProcessor     = _MenuProcessor;
		m_HapticProcessor   = _HapticProcessor;
		m_ItemFactory       = _ItemFactory;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<PurchaseSignal>(RegisterPurchase);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<PurchaseSignal>(RegisterPurchase);
	}

	void RegisterPurchase()
	{
		Hide();
	}

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Background.Setup(m_ProductID, true);
		m_Thumbnail.Setup(m_ProductID);
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
	}

	public void Next()
	{
		m_ProductID = m_PurchaseProcessor.GetNextProductID(m_ProductID);
		
		m_Background.Setup(m_ProductID);
		m_Thumbnail.Setup(m_ProductID);
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
		
		m_PreviewSource.Stop();
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
	}

	public void Previous()
	{
		m_ProductID = m_PurchaseProcessor.GetPreviousProductID(m_ProductID);
		
		m_Background.Setup(m_ProductID);
		m_Thumbnail.Setup(m_ProductID);
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
		
		m_PreviewSource.Stop();
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
	}

	public void Purchase()
	{
		m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		m_PurchaseProcessor.Purchase(
			m_ProductID,
			_ProductID => m_MenuProcessor.Hide(MenuType.ProcessingMenu),
			_ProductID => m_MenuProcessor.Hide(MenuType.ProcessingMenu),
			_ProductID => m_MenuProcessor.Hide(MenuType.ProcessingMenu)
		);
		
		m_PreviewSource.Stop();
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
	}

	protected override void OnShowStarted()
	{
		m_PreviewSource.Stop();
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
		
		Refresh();
	}

	protected override void OnHideStarted()
	{
		m_PreviewSource.Stop();
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
	}

	void Refresh()
	{
		if (m_PurchaseProcessor == null)
			return;
		
		string[] levelIDs = m_PurchaseProcessor.GetLevelIDs(m_ProductID)
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
}