using System;
using System.Collections.Generic;
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

	public void ProcessURL(string _URL)
	{
		string                     anchor     = GetAnchor(_URL);
		string                     function   = GetFunction(_URL);
		Dictionary<string, string> parameters = GetParameters(_URL);
		
		Debug.LogFormat("[UrlProcessor] Process URL. Anchor: {0} Function: {1}", anchor, function);
		
		switch (function)
		{
			case "level":
				ProcessLevel(parameters);
				break;
			case "product":
				ProcessProduct(parameters);
				break;
			case "promo":
				ProcessPromo(parameters);
				break;
		}
	}

	static string GetAnchor(string _URL)
	{
		string[] data = _URL.Split(new string[] { "://", "?" }, StringSplitOptions.RemoveEmptyEntries);
		
		return data.Length >= 1 ? data[0] : string.Empty;
	}

	static string GetFunction(string _URL)
	{
		string[] data = _URL.Split(new string[] { "://", "?" }, StringSplitOptions.RemoveEmptyEntries);
		
		return data.Length >= 2 ? data[1] : string.Empty;
	}

	static Dictionary<string, string> GetParameters(string _URL)
	{
		string[] data = _URL.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
		
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		if (data.Length >= 2)
		{
			for (int i = 1; i < data.Length; i++)
			{
				string[] parameter = data[i].Split('=');
				
				if (parameter.Length >= 2)
					parameters[parameter[0]] = parameter[1];
			}
		}
		return parameters;
	}

	void ProcessLevel(Dictionary<string, string> _Parameters)
	{
		if (_Parameters == null || !_Parameters.TryGetValue("level_id", out string levelID))
			return;
		
		UILevelMenu levelMenu = m_MenuProcessor.GetMenu<UILevelMenu>();
		if (levelMenu != null)
			levelMenu.Setup(levelID);
		
		m_MenuProcessor.Show(MenuType.LevelMenu, true);
	}

	void ProcessProduct(Dictionary<string, string> _Parameters)
	{
		if (_Parameters == null || !_Parameters.TryGetValue("product_id", out string productID))
			return;
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		if (productMenu != null)
			productMenu.Setup(productID);
		
		m_MenuProcessor.Hide(MenuType.MainMenu, true);
		m_MenuProcessor.Show(MenuType.ShopMenu, true);
		m_MenuProcessor.Show(MenuType.ProductMenu, true);
	}

	void ProcessPromo(Dictionary<string, string> _Parameters)
	{
		
	}
}
