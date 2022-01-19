using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using Zenject;

public class NewsDataUpdateSignal { }

public class NewsSnapshot
{
	public string ID       { get; }
	public string Language { get; }
	public string Title    { get; }
	public string Text     { get; }
	public string URL      { get; }

	public NewsSnapshot(DataSnapshot _Data)
	{
		ID       = _Data.Key;
		Language = _Data.GetString("language");
		Title    = _Data.GetString("title");
		Text     = _Data.GetString("description");
		URL      = _Data.GetString("url");
	}
}

public class NewsProcessor
{
	public bool Loaded { get; private set; }

	readonly SignalBus         m_SignalBus;
	readonly LanguageProcessor m_LanguageProcessor;

	readonly List<string>                     m_NewsIDs       = new List<string>();
	readonly Dictionary<string, NewsSnapshot> m_NewsSnapshots = new Dictionary<string, NewsSnapshot>();

	DatabaseReference m_NewsData;

	[Inject]
	public NewsProcessor(
		SignalBus         _SignalBus,
		LanguageProcessor _LanguageProcessor
	)
	{
		m_SignalBus         = _SignalBus;
		m_LanguageProcessor = _LanguageProcessor;
	}

	public async Task LoadNews()
	{
		if (m_NewsData == null)
			m_NewsData = FirebaseDatabase.DefaultInstance.RootReference.Child("news");
		
		await FetchNews();
		
		if (Loaded)
			return;
		
		Loaded = true;
		
		m_NewsData.ValueChanged += OnNewsUpdate;
	}

	public List<string> GetNewsIDs()
	{
		return m_NewsIDs.Where(
			_NewsID =>
			{
				NewsSnapshot newsSnapshot = GetNewsSnapshot(_NewsID);
				
				return newsSnapshot != null && m_LanguageProcessor.SupportsLanguage(newsSnapshot.Language);
			}
		).ToList();
	}

	public string GetTitle(string _NewsID)
	{
		NewsSnapshot newsSnapshot = GetNewsSnapshot(_NewsID);
		
		if (newsSnapshot == null)
		{
			Debug.LogErrorFormat("[NewsProcessor] Get title failed. News with ID '{0}' is null.", _NewsID);
			return string.Empty;
		}
		
		return newsSnapshot.Title;
	}

	public string GetText(string _NewsID)
	{
		NewsSnapshot newsSnapshot = GetNewsSnapshot(_NewsID);
		
		if (newsSnapshot == null)
		{
			Debug.LogErrorFormat("[NewsProcessor] Get text failed. News with ID '{0}' is null.", _NewsID);
			return string.Empty;
		}
		
		return newsSnapshot.Text;
	}

	public string GetURL(string _NewsID)
	{
		NewsSnapshot newsSnapshot = GetNewsSnapshot(_NewsID);
		
		if (newsSnapshot == null)
		{
			Debug.LogErrorFormat("[NewsProcessor] Get URL failed. News with ID '{0}' is null.", _NewsID);
			return string.Empty;
		}
		
		return newsSnapshot.URL;
	}

	async void OnNewsUpdate(object _Sender, EventArgs _Args)
	{
		Debug.Log("[NewsProcessor] Updating news data...");
		
		await FetchNews();
		
		Debug.Log("[NewsProcessor] Update news data complete.");
		
		m_SignalBus.Fire<NewsDataUpdateSignal>();
	}

	async Task FetchNews()
	{
		m_NewsIDs.Clear();
		m_NewsSnapshots.Clear();
		
		DataSnapshot newsSnapshots = await m_NewsData.OrderByChild("order").GetValueAsync();
		
		foreach (DataSnapshot newsSnapshot in newsSnapshots.Children)
		{
			bool active = newsSnapshot.GetBool("active");
			
			if (!active)
				continue;
			
			NewsSnapshot news = new NewsSnapshot(newsSnapshot);
			
			m_NewsIDs.Add(news.ID);
			m_NewsSnapshots[news.ID] = news;
		}
	}

	NewsSnapshot GetNewsSnapshot(string _NewsID)
	{
		if (string.IsNullOrEmpty(_NewsID))
		{
			Debug.LogError("[NewsProcessor] Get news snapshot failed. News ID is null or empty.");
			return null;
		}
		
		if (!m_NewsSnapshots.ContainsKey(_NewsID))
		{
			Debug.LogErrorFormat("[NewsProcessor] Get news snapshot failed. News with ID '{0}' not found.", _NewsID);
			return null;
		}
		
		return m_NewsSnapshots[_NewsID];
	}
}
