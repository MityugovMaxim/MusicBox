using System;
using Firebase.DynamicLinks;
using UnityEngine;
using Zenject;

public enum MainMenuPageType
{
	News    = 0,
	Store   = 1,
	Songs   = 2,
	Profile = 3,
	Offers  = 4,
}

[Menu(MenuType.MainMenu)]
public class UIMainMenu : UIMenu
{
	[SerializeField] UIProfile         m_Profile;
	[SerializeField] UIMainMenuPage[]  m_Pages;
	[SerializeField] UIMainMenuControl m_Control;

	[Inject] SignalBus    m_SignalBus;
	[Inject] UrlProcessor m_UrlProcessor;

	[NonSerialized] MainMenuPageType m_PageType = MainMenuPageType.Songs;

	public void Select(MainMenuPageType _PageType, bool _Instant = false)
	{
		if (m_PageType == _PageType)
			return;
		
		foreach (UIMainMenuPage page in m_Pages)
		{
			if (page.Type == _PageType)
				page.Show(m_PageType, _Instant);
			else
				page.Hide(_PageType, _Instant);
		}
		
		m_PageType = _PageType;
		
		m_Control.Select(m_PageType, _Instant);
	}

	protected override void OnShowStarted()
	{
		Refresh();
		
		foreach (UIMainMenuPage page in m_Pages)
		{
			if (page.Type == m_PageType)
				page.Show(m_PageType, true);
			else
				page.Hide(m_PageType, true);
		}
		m_Control.Select(m_PageType, true);
		
		Application.deepLinkActivated    += ProcessDeepLink;
		DynamicLinks.DynamicLinkReceived += ProcessDynamicLink;
		
		if (m_SignalBus == null)
			return;
		
		m_SignalBus.Subscribe<SocialDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ScoresDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProductsDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProgressDataUpdateSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		Application.deepLinkActivated    -= ProcessDeepLink;
		DynamicLinks.DynamicLinkReceived -= ProcessDynamicLink;
		
		if (m_SignalBus == null)
			return;
		
		m_SignalBus.Unsubscribe<SocialDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ScoresDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProductsDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProgressDataUpdateSignal>(Refresh);
	}

	protected override void OnHideFinished()
	{
		foreach (UIMainMenuPage page in m_Pages)
			page.Hide(m_PageType, true);
	}

	void Refresh()
	{
		m_Profile.Setup();
	}

	async void ProcessDeepLink(string _URL)
	{
		await m_UrlProcessor.ProcessURL(_URL);
	}

	async void ProcessDynamicLink(object _Sender, ReceivedDynamicLinkEventArgs _Args)
	{
		await m_UrlProcessor.ProcessDynamicLink(_Args.ReceivedDynamicLink.Url);
	}
}
