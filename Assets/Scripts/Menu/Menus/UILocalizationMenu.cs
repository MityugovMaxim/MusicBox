using System;
using System.Collections.Generic;
using System.Linq;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

[Menu(MenuType.LocalizationMenu)]
public class UILocalizationMenu : UIMenu
{
	const string DEFAULT_KEY   = "UNASSIGNED_KEY";
	const string DEFAULT_VALUE = "UNASSIGNED_VALUE";

	[SerializeField] UILocalizationItem m_Item;
	[SerializeField] RectTransform      m_Container;

	[Inject] LanguageProcessor     m_LanguageProcessor;
	[Inject] LocalizationProcessor m_LocalizationProcessor;
	[Inject] MenuProcessor         m_MenuProcessor;

	readonly List<UILocalizationItem> m_Items = new List<UILocalizationItem>();

	public async void Back()
	{
		await m_MenuProcessor.Hide(MenuType.LocalizationMenu);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_LocalizationProcessor.Restore();
		
		Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public void Add()
	{
		UILocalizationItem item = Instantiate(m_Item, m_Container, false);
		
		if (item == null)
			return;
		
		Dictionary<string, string> localization = m_LocalizationProcessor.GetLocalization();
		
		string key = DEFAULT_KEY.ToUnique('_', localization.ContainsKey);
		
		localization[key] = DEFAULT_VALUE;
		
		m_LocalizationProcessor.SetLocalization(localization);
		
		item.BringToBack();
		
		item.Setup(
			key,
			DEFAULT_VALUE,
			Open,
			Remove
		);
		
		m_Items.Insert(0, item);
	}

	public async void Sync()
	{
		Dictionary<string, string> localization = m_LocalizationProcessor.GetLocalization();
		
		List<string> languages = m_LanguageProcessor.GetLanguages();
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		foreach (string language in languages)
		{
			if (language == m_LanguageProcessor.Language)
				continue;
			
			try
			{
				await m_LocalizationProcessor.Load(language);
			}
			catch (Exception)
			{
				continue;
			}
			
			Dictionary<string, string> entries = m_LocalizationProcessor.GetLocalization();
			
			foreach (var entry in entries)
			{
				if (localization.ContainsKey(entry.Key))
					continue;
				
				localization[entry.Key] = DEFAULT_VALUE;
			}
		}
		
		await m_LocalizationProcessor.Load(m_LanguageProcessor.Language);
		
		m_LocalizationProcessor.SetLocalization(localization);
		
		try
		{
			await m_LocalizationProcessor.Upload();
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload localization failed. Language: '{0}'.", m_LanguageProcessor.Language);
			
			await m_MenuProcessor.ExceptionAsync("Upload failed", exception);
		}
		
		Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Upload()
	{
		Dictionary<string, string> localization = m_Items
			.Where(_Item => _Item != null)
			.ToDictionary(_Item => _Item.Key, _Item => _Item.Value);
		
		m_LocalizationProcessor.SetLocalization(localization);
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_LocalizationProcessor.Upload();
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload localization failed. Language: '{0}'.", m_LanguageProcessor.Language);
			
			await m_MenuProcessor.ExceptionAsync("Upload failed", exception);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	protected override void OnShowStarted()
	{
		Refresh();
	}

	void Refresh()
	{
		foreach (UILocalizationItem item in m_Items)
			Destroy(item.gameObject);
		m_Items.Clear();
		
		Dictionary<string, string> localization = m_LocalizationProcessor.GetLocalization();
		
		if (localization == null)
			return;
		
		foreach (var entry in localization)
		{
			UILocalizationItem item = Instantiate(m_Item, m_Container, false);
			
			if (item == null)
				continue;
			
			item.Setup(
				entry.Key,
				entry.Value,
				Open,
				Remove
			);
			
			m_Items.Add(item);
		}
	}

	async void Open(string _Key)
	{
		UILocalizationItem item = m_Items.FirstOrDefault(_Item => _Item.Key == _Key);
		
		if (item == null)
			return;
		
		UILocalizationSettingsMenu localizationSettingsMenu = m_MenuProcessor.GetMenu<UILocalizationSettingsMenu>();
		
		localizationSettingsMenu.Setup(item.Key, item.Value);
		
		await m_MenuProcessor.Show(MenuType.LocalizationSettingsMenu);
		await m_MenuProcessor.Hide(MenuType.LocalizationMenu, true);
	}

	void Remove(string _Key)
	{
		bool Predicate(UILocalizationItem _Item) => _Item.Key == _Key;
		
		List<UILocalizationItem> items = m_Items.FindAll(Predicate);
		
		m_Items.RemoveAll(Predicate);
		
		foreach (UILocalizationItem item in items)
			Destroy(item.gameObject);
	}
}