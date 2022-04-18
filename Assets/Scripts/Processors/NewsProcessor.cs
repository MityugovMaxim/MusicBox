using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class NewsSnapshot
{
	public string ID        { get; }
	public bool   Active    { get; }
	public string Image     { get; }
	public long   Timestamp { get; }
	public string URL       { get; }
	public int    Order     { get; }

	public NewsSnapshot(DataSnapshot _Data)
	{
		ID        = _Data.Key;
		Active    = _Data.GetBool("active");
		Image     = _Data.GetString("image");
		Timestamp = _Data.GetLong("timestamp");
		URL       = _Data.GetString("url");
		Order     = _Data.GetInt("order");
	}
}

[Preserve]
public class NewsDataUpdateSignal { }

[Preserve]
public class NewsDescriptor : DescriptorProcessor<NewsDataUpdateSignal>
{
	protected override string Path => "news_descriptors";
}

[Preserve]
public class NewsProcessor
{
	bool Loaded { get; set; }

	[Inject] SignalBus      m_SignalBus;
	[Inject] NewsDescriptor m_NewsDescriptor;

	readonly List<NewsSnapshot> m_Snapshots = new List<NewsSnapshot>();

	DatabaseReference m_Data;

	public async Task Load()
	{
		if (m_Data == null)
		{
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child("news");
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		await m_NewsDescriptor.Load();
		
		Loaded = true;
	}

	public List<string> GetNewsIDs()
	{
		return m_Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.OrderByDescending(_Snapshot => _Snapshot.Timestamp)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetImage(string _NewsID)
	{
		NewsSnapshot snapshot = GetSnapshot(_NewsID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public string GetTitle(string _NewsID) => m_NewsDescriptor.GetTitle(_NewsID);

	public string GetDescription(string _NewsID) => m_NewsDescriptor.GetDescription(_NewsID);

	public string GetDate(string _NewsID)
	{
		NewsSnapshot snapshot = GetSnapshot(_NewsID);
		
		if (snapshot == null || snapshot.Timestamp == 0)
			return string.Empty;
		
		DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(snapshot.Timestamp);
		
		return date.LocalDateTime.ToShortDateString();
	}

	public string GetURL(string _NewsID)
	{
		NewsSnapshot snapshot = GetSnapshot(_NewsID);
		
		return snapshot?.URL ?? string.Empty;
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[NewsProcessor] Updating news data...");
		
		await Fetch();
		
		Debug.Log("[NewsProcessor] Update news data complete.");
		
		m_SignalBus.Fire<NewsDataUpdateSignal>();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.OrderByChild("order").GetValueAsync(15000, 2);
		
		if (dataSnapshot == null)
		{
			Debug.LogError("[NewsProcessor] Fetch news failed.");
			return;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(_Data => new NewsSnapshot(_Data)));
	}

	NewsSnapshot GetSnapshot(string _NewsID)
	{
		if (m_Snapshots.Count == 0)
			return null;
		
		if (string.IsNullOrEmpty(_NewsID))
		{
			Debug.LogError("[NewsProcessor] Get snapshot failed. News ID is null or empty.");
			return null;
		}
		
		NewsSnapshot snapshot = m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _NewsID);
		
		if (snapshot == null)
			Debug.LogErrorFormat("[NewsProcessor] Get snapshot failed. Snapshot with ID '{0}' is null.", _NewsID);
		
		return snapshot;
	}
}
