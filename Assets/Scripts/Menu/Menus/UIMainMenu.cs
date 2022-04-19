using System;
using System.Linq;
using UnityEngine;
using Zenject;

public enum MainMenuPageType
{
	News    = 0,
	Store   = 1,
	Songs  = 2,
	Profile = 3,
	Offers  = 4,
}

[Menu(MenuType.MainMenu)]
public class UIMainMenu : UIMenu
{
	[SerializeField] UIProfile         m_Profile;
	[SerializeField] UIProductPromo    m_ProductPromo;
	[SerializeField] UIMainMenuPage[]  m_Pages;
	[SerializeField] UIMainMenuControl m_Control;

	SignalBus        m_SignalBus;
	ProfileProcessor m_ProfileProcessor;
	ProductsProcessor m_ProductsProcessor;
	UrlProcessor     m_UrlProcessor;

	[NonSerialized] MainMenuPageType m_PageType = MainMenuPageType.Songs;

	[Inject]
	public void Construct(
		SignalBus          _SignalBus,
		ProfileProcessor   _ProfileProcessor,
		ProductsProcessor   _ProductsProcessor,
		UrlProcessor       _UrlProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_SignalBus        = _SignalBus;
		m_ProfileProcessor = _ProfileProcessor;
		m_ProductsProcessor = _ProductsProcessor;
		m_UrlProcessor     = _UrlProcessor;
	}

	public void Select(MainMenuPageType _PageType, bool _Instant = false)
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
		
		m_SignalBus.Subscribe<SocialDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ScoresDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProductsDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProgressDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<SocialDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ScoresDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProductsDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProgressDataUpdateSignal>(Refresh);
	}

	protected override void OnHideFinished()
	{
		foreach (UIMainMenuPage page in m_Pages)
			page.Hide(true);
	}

	void Refresh()
	{
		string productID = m_ProfileProcessor.GetVisibleProductIDs().FirstOrDefault(m_ProductsProcessor.IsPromo);
		
		m_Profile.Setup();
		m_ProductPromo.Setup(productID);
	}

	async void ProcessDeepLink(string _URL)
	{
		await m_UrlProcessor.ProcessURL(_URL);
	}
}
