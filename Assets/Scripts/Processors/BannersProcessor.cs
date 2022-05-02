using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class BannerSnapshot
{
	public string ID        { get; set; }
	public bool   Active    { get; set; }
	public string Language  { get; }
	public bool   Permanent { get; set; }
	public string URL       { get; set; }
	public int    Order     { get; set; }

	public BannerSnapshot(string _BannerID, string _Language)
	{
		ID       = _BannerID;
		Language = _Language;
		URL      = string.Empty;
	}

	public BannerSnapshot(DataSnapshot _Data)
	{
		ID        = _Data.Key;
		Active    = _Data.GetBool("active");
		Language  = _Data.GetString("language");
		Permanent = _Data.GetBool("permanent");
		URL       = _Data.GetString("url");
	}

	public Dictionary<string, object> Serialize()
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		data["active"]    = Active;
		data["language"]  = Language;
		data["permanent"] = Permanent;
		data["url"]       = URL;
		data["order"]     = Order;
		
		return data;
	}
}

[Preserve]
public class BannersDataUpdateSignal { }

[Preserve]
public class BannersProcessor : IInitializable, IDisposable
{
	bool Loaded { get; set; }

	[Inject] SignalBus         m_SignalBus;
	[Inject] LanguageProcessor m_LanguageProcessor;

	DatabaseReference m_Data;

	readonly List<BannerSnapshot> m_Snapshots = new List<BannerSnapshot>();

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LanguageSelectSignal>(OnLanguageSelect);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LanguageSelectSignal>(OnLanguageSelect);
	}

	public async Task Load()
	{
		if (m_Data == null)
		{
			string path = $"banners/{m_LanguageProcessor.Language}";
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child(path);
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		Loaded = true;
	}

	public List<string> GetBannerIDs()
	{
		return m_Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetURL(string _BannerID)
	{
		BannerSnapshot snapshot = GetSnapshot(_BannerID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[BannersProcessor] Get URL failed. Snapshot with ID '{0}' is null.", _BannerID);
			return string.Empty;
		}
		
		return snapshot.URL;
	}

	public bool IsPermanent(string _BannerID)
	{
		BannerSnapshot snapshot = GetSnapshot(_BannerID);
		
		if (snapshot == null)
		{
			Debug.LogErrorFormat("[BannersProcessor] Check permanent failed. Snapshot with ID '{0}' is null.", _BannerID);
			return false;
		}
		
		return snapshot.Permanent;
	}

	async void OnLanguageSelect()
	{
		if (m_Data == null)
			return;
		
		m_Data.ValueChanged -= OnUpdate;
		m_Data              =  null;
		Loaded              =  false;
		
		await Load();
		
		m_SignalBus.Fire<BannersDataUpdateSignal>();
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[BannersProcessor] Updating banners data...");
		
		await Fetch();
		
		Debug.Log("[BannersProcessor] Update banners data complete.");
		
		m_SignalBus.Fire<BannersDataUpdateSignal>();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.OrderByChild("order").GetValueAsync(15000, 4);
		
		if (dataSnapshot == null)
		{
			Debug.LogError("[BannersProcessor] Fetch banners failed.");
			return;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(_Data => new BannerSnapshot(_Data)));
	}

	public async Task Upload()
	{
		Loaded = false;
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (BannerSnapshot snapshot in m_Snapshots)
		{
			if (snapshot != null)
				data[snapshot.ID] = snapshot.Serialize();
		}
		
		await m_Data.SetValueAsync(data);
		
		await Fetch();
		
		Loaded = true;
		
		m_SignalBus.Fire<BannersDataUpdateSignal>();
	}

	public async Task Upload(params string[] _BannerIDs)
	{
		if (_BannerIDs == null || _BannerIDs.Length == 0)
			return;
		
		Loaded = false;
		
		foreach (string bannerID in _BannerIDs)
		{
			BannerSnapshot snapshot = GetSnapshot(bannerID);
			
			Dictionary<string, object> data = snapshot?.Serialize();
			
			await m_Data.Child(bannerID).SetValueAsync(data);
		}
		
		await Fetch();
		
		Loaded = true;
		
		m_SignalBus.Fire<BannersDataUpdateSignal>();
	}

	public void MoveSnapshot(string _BannerID, int _Offset)
	{
		int sourceIndex = m_Snapshots.FindIndex(_Snapshot => _Snapshot.ID == _BannerID);
		int targetIndex = sourceIndex + _Offset;
		
		if (sourceIndex < 0 || sourceIndex >= m_Snapshots.Count || targetIndex < 0 || targetIndex >= m_Snapshots.Count)
			return;
		
		(m_Snapshots[sourceIndex], m_Snapshots[targetIndex]) = (m_Snapshots[targetIndex], m_Snapshots[sourceIndex]);
		
		for (int i = 0; i < m_Snapshots.Count; i++)
			m_Snapshots[i].Order = i;
		
		m_SignalBus.Fire<BannersDataUpdateSignal>();
	}

	public BannerSnapshot CreateSnapshot(string _BannerID)
	{
		string bannerID = _BannerID.ToUnique('_', GetBannerIDs());
		string language = m_LanguageProcessor.Language;
		
		BannerSnapshot snapshot = new BannerSnapshot(bannerID, language);
		
		m_Snapshots.Insert(0, snapshot);
		
		return snapshot;
	}

	public void RemoveSnapshot(string _BannerID)
	{
		m_Snapshots.RemoveAll(_Snapshot => _Snapshot.ID == _BannerID);
		
		m_SignalBus.Fire<BannersDataUpdateSignal>();
	}

	public BannerSnapshot GetSnapshot(string _BannerID)
	{
		if (string.IsNullOrEmpty(_BannerID))
		{
			Debug.LogError("[BannersProcessor] Get banner snapshot failed. Banner ID is null or empty.");
			return null;
		}
		
		return m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _BannerID);
	}
}