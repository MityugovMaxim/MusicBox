using System;
using UnityEngine;
using Zenject;

public enum MainMenuPageType
{
	News    = 0,
	Store   = 1,
	Songs   = 2,
	Profile = 3,
	Season  = 4,
}

[Menu(MenuType.MainMenu)]
public class UIMainMenu : UIMenu
{
	[SerializeField] UIMainMenuPage[]  m_Pages;
	[SerializeField] UIMainMenuControl m_Control;

	[Inject] SignalBus     m_SignalBus;
	[Inject] LinkProcessor m_LinkProcessor;

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

	protected override async void OnShowStarted()
	{
		foreach (UIMainMenuPage page in m_Pages)
		{
			if (page.Type == m_PageType)
				page.Show(m_PageType, true);
			else
				page.Hide(m_PageType, true);
		}
		m_Control.Select(m_PageType, true);
		
		await m_LinkProcessor.Process(true);
		
		if (m_SignalBus == null)
			return;
		
		m_SignalBus.Subscribe<LinkReceivedSignal>(ProcessLink);
	}

	protected override void OnHideStarted()
	{
		if (m_SignalBus == null)
			return;
		
		m_SignalBus.Unsubscribe<LinkReceivedSignal>(ProcessLink);
	}

	protected override void OnHideFinished()
	{
		foreach (UIMainMenuPage page in m_Pages)
			page.Hide(m_PageType, true);
	}

	async void ProcessLink() => await m_LinkProcessor.Process();
}
