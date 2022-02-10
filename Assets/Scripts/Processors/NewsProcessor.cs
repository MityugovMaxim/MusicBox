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
	public bool   Active   { get; }
	public string Image    { get; }
	public string Language { get; }
	public string Title    { get; }
	public string Text     { get; }
	public string URL      { get; }

	public NewsSnapshot(DataSnapshot _Data)
	{
		ID       = _Data.Key;
		Active   = _Data.GetBool("active");
		Image    = _Data.GetString("image");
		Language = _Data.GetString("language");
		Title    = _Data.GetString("title");
		Text     = _Data.GetString("description");
		URL      = _Data.GetString("url");
	}
}

public class NewsProcessor
{
	bool Loaded { get; set; }

	readonly SignalBus         m_SignalBus;
	readonly LanguageProcessor m_LanguageProcessor;

	readonly List<NewsSnapshot> m_NewsSnapshots = new List<NewsSnapshot>();

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
		{
			m_NewsData              =  FirebaseDatabase.DefaultInstance.RootReference.Child("news");
			m_NewsData.ValueChanged += OnNewsUpdate;
		}
		
		await FetchNews();
		
		Loaded = true;
	}

	public List<string> GetNewsIDs()
	{
		return m_NewsSnapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => m_LanguageProcessor.SupportsLanguage(_Snapshot.Language))
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetImage(string _NewsID)
	{
		NewsSnapshot snapshot = GetNewsSnapshot(_NewsID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[NewsProcessor] Get image failed. News with ID '{0}' is null.", _NewsID);
			return string.Empty;
		}
		
		return snapshot.Image;
	}

	public string GetTitle(string _NewsID)
	{
		NewsSnapshot snapshot = GetNewsSnapshot(_NewsID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[NewsProcessor] Get title failed. Snapshot with ID '{0}' is null.", _NewsID);
			return string.Empty;
		}
		
		return snapshot.Title;
	}

	public string GetText(string _NewsID)
	{
		NewsSnapshot snapshot = GetNewsSnapshot(_NewsID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[NewsProcessor] Get text failed. Snapshot with ID '{0}' is null.", _NewsID);
			return string.Empty;
		}
		
		return snapshot.Text;
	}

	public string GetURL(string _NewsID)
	{
		NewsSnapshot snapshot = GetNewsSnapshot(_NewsID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[NewsProcessor] Get URL failed. Snapshot with ID '{0}' is null.", _NewsID);
			return string.Empty;
		}
		
		return snapshot.URL;
	}

	async void OnNewsUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[NewsProcessor] Updating news data...");
		
		await FetchNews();
		
		Debug.Log("[NewsProcessor] Update news data complete.");
		
		m_SignalBus.Fire<NewsDataUpdateSignal>();
	}

	async Task FetchNews()
	{
		m_NewsSnapshots.Clear();
		
		DataSnapshot data = await m_NewsData.OrderByChild("order").GetValueAsync(15000, 2);
		
		if (data == null)
		{
			Debug.LogError("[NewsProcessor] Fetch news failed.");
			return;
		}
		
		m_NewsSnapshots.AddRange(data.Children.Select(_Data => new NewsSnapshot(_Data)));
	}

	NewsSnapshot GetNewsSnapshot(string _NewsID)
	{
		if (m_NewsSnapshots == null || m_NewsSnapshots.Count == 0)
			return null;
		
		if (string.IsNullOrEmpty(_NewsID))
		{
			Debug.LogError("[NewsProcessor] Get news snapshot failed. News ID is null or empty.");
			return null;
		}
		
		return m_NewsSnapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _NewsID);
	}
}
