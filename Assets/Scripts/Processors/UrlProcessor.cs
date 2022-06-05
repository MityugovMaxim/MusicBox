using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

	public async Task ProcessURL(string _URL, bool _Instant = false)
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
				await ProcessNews(parameters, _Instant);
				break;
			case "songs":
				await ProcessSongs(parameters, _Instant);
				break;
			case "store":
				await ProcessStore(parameters, _Instant);
				break;
			case "offers":
				await ProcessOffers(parameters, _Instant);
				break;
			case "profile":
				await ProcessProfile(parameters, _Instant);
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

	async Task ProcessStore(Dictionary<string, string> _Parameters, bool _Instant)
	{
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		
		if (mainMenu == null || !mainMenu.Shown)
			return;
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		
		if (_Parameters == null || !_Parameters.TryGetValue("product_id", out string productID))
		{
			await SelectMainPage(MainMenuPageType.Store, _Parameters, _Instant);
			return;
		}
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.SongMenu, _Instant);
		
		await m_MenuProcessor.Hide(MenuType.ProductMenu, _Instant);
		
		if (productMenu != null)
			productMenu.Setup(productID);
		
		await m_MenuProcessor.Show(MenuType.ProductMenu, _Instant);
		
		mainMenu.Select(MainMenuPageType.Songs, _Instant);
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	async Task ProcessSongs(Dictionary<string, string> _Parameters, bool _Instant)
	{
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		
		if (mainMenu == null || !mainMenu.Shown)
			return;
		
		UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
		
		if (_Parameters == null || !_Parameters.TryGetValue("song_id", out string songID))
		{
			await SelectMainPage(MainMenuPageType.Songs, _Parameters, _Instant);
			return;
		}
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.SongMenu, _Instant);
		
		await m_MenuProcessor.Hide(MenuType.ProductMenu, _Instant);
		
		if (songMenu != null)
			songMenu.Setup(songID);
		
		await m_MenuProcessor.Show(MenuType.SongMenu, _Instant);
		
		mainMenu.Select(MainMenuPageType.Songs, _Instant);
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	Task ProcessNews(Dictionary<string, string> _Parameters, bool _Instant)
	{
		return SelectMainPage(MainMenuPageType.News, _Parameters, _Instant);
	}

	Task ProcessOffers(Dictionary<string, string> _Parameters, bool _Instant)
	{
		return SelectMainPage(MainMenuPageType.Offers, _Parameters, _Instant);
	}

	Task ProcessProfile(Dictionary<string, string> _Parameters, bool _Instant)
	{
		return SelectMainPage(MainMenuPageType.Profile, _Parameters, _Instant);
	}

	[SuppressMessage("ReSharper", "UnusedParameter.Local")]
	async Task SelectMainPage(MainMenuPageType _PageType, Dictionary<string, string> _Parameters, bool _Instant)
	{
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		
		if (mainMenu == null || !mainMenu.Shown)
			return;
		
		await m_MenuProcessor.Hide(MenuType.SongMenu, _Instant);
		
		await m_MenuProcessor.Hide(MenuType.ProductMenu, _Instant);
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		if (mainMenu != null)
			mainMenu.Select(_PageType, _Instant);
	}
}
