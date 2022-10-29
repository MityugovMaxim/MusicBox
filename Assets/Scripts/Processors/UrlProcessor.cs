using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.DynamicLinks;
using UnityEngine;
using Zenject;

public class UrlProcessor
{
	const string SCHEME     = "audiobox";
	const string URL_PREFIX = "https://audiobox.page.link";
	const string URL_HOST   = "https://outofbounds.studio/audiobox?";

	[Inject] MenuProcessor  m_MenuProcessor;
	[Inject] SongsProcessor m_SongsProcessor;
	[Inject] SongsManager   m_SongsManager;

	public async Task<string> GenerateDynamicLink(string _Payload = null)
	{
		if (_Payload == null)
			_Payload = string.Empty;
		
		DynamicLinkComponents components = new DynamicLinkComponents(
			new Uri(URL_HOST + _Payload),
			URL_PREFIX
		);
		components.IOSParameters     = new IOSParameters(Application.identifier);
		components.AndroidParameters = new AndroidParameters(Application.identifier);
		
		DynamicLinkOptions options = new DynamicLinkOptions();
		options.PathLength = DynamicLinkPathLength.Short;
		
		ShortDynamicLink link = await DynamicLinks.GetShortLinkAsync(components, options);
		
		foreach (string warning in link.Warnings)
			Log.Warning(this, warning);
		
		return link.Url.ToString();
	}

	public Task ProcessDynamicLink(Uri _URL, bool _Instant = false)
	{
		if (_URL == null)
			return Task.CompletedTask;
		
		if (string.IsNullOrEmpty(_URL.Query))
			return Task.CompletedTask;
		
		string url = $"{SCHEME}://{_URL.Query.Replace("audiobox?", string.Empty)}";
		
		return ProcessURL(url, _Instant);
	}

	public async Task ProcessURL(string _URL, bool _Instant = false)
	{
		if (string.IsNullOrEmpty(_URL))
			return;
		
		Uri uri = new Uri(_URL);
		
		if (uri.Scheme != SCHEME)
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
			case "play":
				await ProcessPlay(parameters, _Instant);
				break;
			default:
				await ProcessHash(uri.Host, _Instant);
				break;
		}
	}

	public string GetSongURL(string _SongID)
	{
		if (string.IsNullOrEmpty(_SongID))
			return $"{SCHEME}://";
		
		return $"{SCHEME}://songs?song_id={_SongID}";
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

	Task ProcessPlay(Dictionary<string, string> _Parameters, bool _Instant)
	{
		string songID = m_SongsManager
			.GetLibrarySongIDs()
			.FirstOrDefault();
		
		if (string.IsNullOrEmpty(songID))
			return Task.CompletedTask;
		
		_Parameters["song_id"] = songID;
		
		return ProcessSongs(_Parameters, _Instant);
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

	Task ProcessHash(string _Hash, bool _Instant)
	{
		string songID = m_SongsProcessor.GetSongID(_Hash);
		
		if (string.IsNullOrEmpty(songID))
			return Task.CompletedTask;
		
		Dictionary<string, string> parameters = new Dictionary<string, string>()
		{
			{ "song_id", songID },
		};
		
		return ProcessSongs(parameters, _Instant);
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
