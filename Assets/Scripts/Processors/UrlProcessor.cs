using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UrlProcessor
{
	readonly MenuProcessor m_MenuProcessor;

	[Inject]
	public UrlProcessor(MenuProcessor _MenuProcessor)
	{
		m_MenuProcessor = _MenuProcessor;
	}

	public async Task ProcessURL(string _URL)
	{
		if (string.IsNullOrEmpty(_URL))
			return;
		
		Uri uri = new Uri(_URL);
		
		if (uri.Scheme != "audiobox")
		{
			Application.OpenURL(_URL);
			return;
		}
		
		Dictionary<string, string> parameters = GetParameters(uri.Query);
		
		switch (uri.Host)
		{
			case "news":
				await ProcessNews();
				break;
			case "level":
				await ProcessLevel(parameters);
				break;
			case "store":
				await ProcessProduct(parameters);
				break;
			case "offers":
				await ProcessOffers();
				break;
			case "profile":
				await ProcessProfile();
				break;
			default:
				Application.OpenURL(_URL);
				break;
		}
	}

	static Dictionary<string, string> GetParameters(string _URL)
	{
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		
		string[] data = _URL.Split(new char[] { '?', '&' }, StringSplitOptions.RemoveEmptyEntries);
		if (data.Length > 0)
		{
			foreach (string parameter in data)
			{
				string[] entry = parameter.Split('=');
				
				if (entry.Length >= 2)
					parameters[entry[0]] = entry[1];
			}
		}
		return parameters;
	}

	Task ProcessNews()
	{
		return SelectMainPage(MainMenuPageType.News);
	}

	async Task ProcessProduct(IReadOnlyDictionary<string, string> _Parameters)
	{
		if (_Parameters == null || !_Parameters.TryGetValue("product_id", out string productID))
			return;
		
		UIMainMenu    mainMenu    = m_MenuProcessor.GetMenu<UIMainMenu>();
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		
		bool instant = mainMenu == null || !mainMenu.Shown;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.LevelMenu);
		
		await m_MenuProcessor.Hide(MenuType.ProductMenu);
		
		if (productMenu != null)
			productMenu.Setup(productID);
		
		await m_MenuProcessor.Show(MenuType.ProductMenu, instant);
		
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Store, true);
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	async Task ProcessLevel(IReadOnlyDictionary<string, string> _Parameters)
	{
		if (_Parameters == null || !_Parameters.TryGetValue("level_id", out string levelID))
			return;
		
		UIMainMenu  mainMenu  = m_MenuProcessor.GetMenu<UIMainMenu>();
		UILevelMenu levelMenu = m_MenuProcessor.GetMenu<UILevelMenu>();
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.LevelMenu);
		
		if (levelMenu != null)
			levelMenu.Setup(levelID);
		
		await m_MenuProcessor.Hide(MenuType.ProductMenu);
		
		await m_MenuProcessor.Show(MenuType.LevelMenu, mainMenu == null || !mainMenu.Shown);
		
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Levels);
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	Task ProcessOffers()
	{
		return SelectMainPage(MainMenuPageType.Offers);
	}

	Task ProcessProfile()
	{
		return SelectMainPage(MainMenuPageType.Profile);
	}

	async Task SelectMainPage(MainMenuPageType _PageType)
	{
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		
		bool instant = mainMenu == null || !mainMenu.Shown;
		
		await m_MenuProcessor.Hide(MenuType.LevelMenu);
		
		await m_MenuProcessor.Hide(MenuType.ProductMenu);
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		if (mainMenu != null)
			mainMenu.Select(_PageType, instant);
	}
}
