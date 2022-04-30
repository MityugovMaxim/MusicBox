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
	public bool   Active    { get; set; }
	public long   Timestamp { get; }
	public string URL       { get; set; }
	[HideProperty]
	public int    Order     { get; set; }

	public NewsSnapshot(string _NewsID)
	{
		ID = _NewsID;
	}

	public NewsSnapshot(DataSnapshot _Data)
	{
		ID        = _Data.Key;
		Active    = _Data.GetBool("active");
		Timestamp = _Data.GetLong("timestamp");
		URL       = _Data.GetString("url");
		Order     = _Data.GetInt("order");
	}

	public Dictionary<string, object> Serialize()
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		data["active"]    = Active;
		data["timestamp"] = Timestamp;
		data["url"]       = URL;
		data["order"]     = Order;
		
		return data;
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
			.OrderBy(_Snapshot => _Snapshot.Order)
			.ThenByDescending(_Snapshot => _Snapshot.Timestamp)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
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

	public async Task Upload()
	{
		Loaded = false;
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (NewsSnapshot snapshot in m_Snapshots)
		{
			if (snapshot != null)
				data[snapshot.ID] = snapshot.Serialize();
		}
		
		await m_Data.SetValueAsync(data);
		
		await m_NewsDescriptor.Upload();
		
		await Fetch();
		
		Loaded = true;
		
		m_SignalBus.Fire<NewsDataUpdateSignal>();
	}

	public async Task Upload(params string[] _NewsIDs)
	{
		if (_NewsIDs == null || _NewsIDs.Length == 0)
			return;
		
		Loaded = false;
		
		foreach (string newsID in _NewsIDs)
		{
			NewsSnapshot snapshot = GetSnapshot(newsID);
			
			Dictionary<string, object> data = snapshot?.Serialize();
			
			await m_Data.Child(newsID).SetValueAsync(data);
		}
		
		await m_NewsDescriptor.Upload(_NewsIDs);
		
		await Fetch();
		
		Loaded = true;
		
		m_SignalBus.Fire<NewsDataUpdateSignal>();
	}

	public void MoveSnapshot(string _NewsID, int _Offset)
	{
		int sourceIndex = m_Snapshots.FindIndex(_Snapshot => _Snapshot.ID == _NewsID);
		int targetIndex = sourceIndex + _Offset;
		
		if (sourceIndex < 0 || sourceIndex >= m_Snapshots.Count || targetIndex < 0 || targetIndex >= m_Snapshots.Count)
			return;
		
		(m_Snapshots[sourceIndex], m_Snapshots[targetIndex]) = (m_Snapshots[targetIndex], m_Snapshots[sourceIndex]);
		
		for (int i = 0; i < m_Snapshots.Count; i++)
			m_Snapshots[i].Order = i;
		
		m_SignalBus.Fire<NewsDataUpdateSignal>();
	}

	public NewsSnapshot CreateSnapshot()
	{
		DatabaseReference reference = m_Data.Push();
		
		string newsID = reference.Key;
		
		NewsSnapshot snapshot = new NewsSnapshot(newsID);
		
		m_Snapshots.Insert(0, snapshot);
		
		m_NewsDescriptor.CreateDescriptor(snapshot.ID);
		
		return snapshot;
	}

	public void RemoveSnapshot(string _NewsID)
	{
		m_Snapshots.RemoveAll(_Snapshot => _Snapshot.ID == _NewsID);
		
		m_NewsDescriptor.RemoveDescriptor(_NewsID);
		
		m_SignalBus.Fire<NewsDataUpdateSignal>();
	}

	public NewsSnapshot GetSnapshot(string _NewsID)
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
