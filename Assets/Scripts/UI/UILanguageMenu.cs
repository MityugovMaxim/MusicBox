using System.Collections.Generic;
using UnityEngine;
using Zenject;

[Menu(MenuType.LanguageMenu)]
public class UILanguageMenu : UISlideMenu
{
	[SerializeField] RectTransform        m_Container;
	[SerializeField] UILanguageBackground m_Background;

	[Inject] SignalBus           m_SignalBus;
	[Inject] LanguageProcessor   m_LanguageProcessor;
	[Inject] MenuProcessor       m_MenuProcessor;
	[Inject] UILanguageItem.Pool m_ItemPool;

	readonly List<UILanguageItem> m_Items = new List<UILanguageItem>();

	string m_Language;

	public void Setup(string _Language)
	{
		m_Language = _Language;
		
		m_Background.Setup(m_Language);
	}

	protected override void OnShowStarted()
	{
		m_SignalBus.Subscribe<LanguageDataUpdateSignal>(Refresh);
		
		Refresh();
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<LanguageDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
		foreach (UILanguageItem item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		List<string> languages = m_LanguageProcessor.GetLanguages();
		
		if (languages == null || languages.Count == 0)
			return;
		
		foreach (string language in languages)
		{
			if (string.IsNullOrEmpty(language))
				continue;
			
			UILanguageItem item = m_ItemPool.Spawn(m_Container);
			
			item.Setup(language, Select);
			
			if (language == m_Language)
				item.BringToBack();
			
			m_Items.Add(item);
		}
	}

	async void Select(string _Language)
	{
		if (m_LanguageProcessor.Language == _Language)
		{
			await m_MenuProcessor.Hide(MenuType.LanguageMenu);
			return;
		}
		
		await m_MenuProcessor.Show(MenuType.LoginMenu);
		
		await m_MenuProcessor.Hide(MenuType.LanguageMenu, true);
		
		await m_LanguageProcessor.Select(_Language);
		
		await m_MenuProcessor.Hide(MenuType.LoginMenu);
	}
}