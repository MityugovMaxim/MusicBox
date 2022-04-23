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
	public string ID        { get; }
	public bool   Active    { get; }
	public string Language  { get; }
	public bool   Permanent { get; }
	public string URL       { get; }

	public BannerSnapshot(DataSnapshot _Data)
	{
		ID        = _Data.Key;
		Active    = _Data.GetBool("active");
		Language  = _Data.GetString("language");
		Permanent = _Data.GetBool("permanent");
		URL       = _Data.GetString("url");
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
			.Where(_Snapshot => _Snapshot.Active)
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

	BannerSnapshot GetSnapshot(string _BannerID)
	{
		if (string.IsNullOrEmpty(_BannerID))
		{
			Debug.LogError("[BannersProcessor] Get banner snapshot failed. Banner ID is null or empty.");
			return null;
		}
		
		return m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _BannerID);
	}
}