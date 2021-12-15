using System;
using System.Linq;
using UnityEngine;
using Zenject;

public enum MainMenuPageType
{
	News    = 0,
	Store   = 1,
	Levels  = 2,
	Profile = 3,
	Offers  = 4,
}

[Menu(MenuType.MainMenu)]
public class UIMainMenu : UIMenu
{
	[SerializeField] UIProductPromo    m_ProductPromo;
	[SerializeField] UIMainMenuPage[]  m_Pages;
	[SerializeField] UIMainMenuControl m_Control;

	SignalBus      m_SignalBus;
	StoreProcessor m_StoreProcessor;
	UrlProcessor   m_UrlProcessor;

	[NonSerialized] MainMenuPageType m_PageType = MainMenuPageType.Levels;

	[Inject]
	public void Construct(
		SignalBus      _SignalBus,
		StoreProcessor _StoreProcessor,
		UrlProcessor   _UrlProcessor
	)
	{
		m_SignalBus      = _SignalBus;
		m_StoreProcessor = _StoreProcessor;
		m_UrlProcessor   = _UrlProcessor;
	}

	public void Select(MainMenuPageType _PageType)
	{
		Select(_PageType, false);
	}

	public void Select(MainMenuPageType _PageType, bool _Instant)
	{
		if (m_PageType == _PageType)
			return;
		
		m_PageType = _PageType;
		
		foreach (UIMainMenuPage page in m_Pages)
		{
			if (page.Type == m_PageType)
				page.Show(_Instant);
			else
				page.Hide(_Instant);
		}
		
		m_Control.Select(m_PageType, _Instant);
	}

	protected override void OnShowStarted()
	{
		Refresh();
		
		foreach (UIMainMenuPage page in m_Pages)
		{
			if (page.Type == m_PageType)
				page.Show(true);
			else
				page.Hide(true);
		}
		m_Control.Select(m_PageType, true);
		
		m_SignalBus.Subscribe<PurchaseSignal>(Refresh);
		m_SignalBus.Subscribe<ProductDataUpdateSignal>(Refresh);
		
		Application.deepLinkActivated += ProcessDeepLink;
	}

	protected override void OnHideStarted()
	{
		Application.deepLinkActivated -= ProcessDeepLink;
	}

	protected override void OnHideFinished()
	{
		foreach (UIMainMenuPage page in m_Pages)
			page.Hide(true);
		
		m_SignalBus.Unsubscribe<PurchaseSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProductDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		string productID = m_StoreProcessor.GetProductIDs()
			.SkipWhile(m_StoreProcessor.IsProductPurchased)
			.FirstOrDefault(m_StoreProcessor.CheckPromo);
		
		if (string.IsNullOrEmpty(productID))
			return;
		
		m_ProductPromo.Setup(productID);
	}

	async void ProcessDeepLink(string _URL)
	{
		await m_UrlProcessor.ProcessURL(_URL);
	}
}
