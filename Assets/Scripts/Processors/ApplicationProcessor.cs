using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using ModestTree.Util;
using UnityEngine;
using Zenject;

public class ApplicationDataUpdateSignal { }

public class ApplicationSnapshot
{
	public string Version { get; }

	public ApplicationSnapshot(DataSnapshot _Data)
	{
		Version = _Data.GetString("version");
	}
}

public class BannerSnapshot
{
	public string ID        { get; }
	public bool   Active    { get; }
	public string Language  { get; }
	public bool   Permanent { get; }
	public string Version   { get; }
	public string URL       { get; }

	public BannerSnapshot(DataSnapshot _Data)
	{
		ID        = _Data.Key;
		Active    = _Data.GetBool("active");
		Language  = _Data.GetString("language");
		Permanent = _Data.GetBool("permanent");
		Version   = _Data.GetString("version");
		URL       = _Data.GetString("url");
	}
}

[Preserve]
public class ApplicationProcessor
{
	bool Loaded { get; set; }

	readonly SignalBus         m_SignalBus;
	readonly LanguageProcessor m_LanguageProcessor;

	readonly List<BannerSnapshot> m_BannerSnapshots = new List<BannerSnapshot>();

	DatabaseReference m_ApplicationData;

	ApplicationSnapshot m_ApplicationSnapshot;

	[Inject]
	public ApplicationProcessor(SignalBus _SignalBus, LanguageProcessor _LanguageProcessor)
	{
		m_SignalBus         = _SignalBus;
		m_LanguageProcessor = _LanguageProcessor;
	}

	public async Task LoadApplication()
	{
		if (m_ApplicationData == null)
		{
			m_ApplicationData              =  FirebaseDatabase.DefaultInstance.RootReference.Child("application");
			m_ApplicationData.ValueChanged += OnApplicationUpdate;
		}
		
		await FetchApplication();
		
		Loaded = true;
	}

	public bool IsOutdated()
	{
		return m_ApplicationSnapshot != null && m_ApplicationSnapshot.Version != Application.version;
	}

	public List<string> GetBannerIDs()
	{
		return m_BannerSnapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => m_LanguageProcessor.SupportsLanguage(_Snapshot.Language))
			.Where(_Snapshot => string.IsNullOrEmpty(_Snapshot.Version) || _Snapshot.Version == Application.version)
			.OrderByDescending(_Snapshot => _Snapshot.Permanent)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetURL(string _BannerID)
	{
		BannerSnapshot bannerSnapshot = GetBannerSnapshot(_BannerID);
		
		if (bannerSnapshot == null)
		{
			Debug.LogErrorFormat("[ApplicationProcessor] Get banner URL failed. Banner snapshot with ID '{0}' is null.", _BannerID);
			return string.Empty;
		}
		
		return bannerSnapshot.URL;
	}

	public bool IsPermanent(string _BannerID)
	{
		BannerSnapshot bannerSnapshot = GetBannerSnapshot(_BannerID);
		
		if (bannerSnapshot == null)
		{
			Debug.LogErrorFormat("[ApplicationProcessor] Get banner permanent flag failed. Banner snapshot with ID '{0}' is null.", _BannerID);
			return false;
		}
		
		return bannerSnapshot.Permanent;
	}

	async void OnApplicationUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[ApplicationProcessor] Updating application data...");
		
		await FetchApplication();
		
		Debug.Log("[ApplicationProcessor] Update application data complete.");
		
		m_SignalBus.Fire<ApplicationDataUpdateSignal>();
	}

	async Task FetchApplication()
	{
		DataSnapshot applicationSnapshot = await m_ApplicationData.GetValueAsync(15000, 4);
		
		if (applicationSnapshot == null)
		{
			Debug.LogError("[ApplicationProcessor] Fetch application failed.");
			return;
		}
		
		m_ApplicationSnapshot = new ApplicationSnapshot(applicationSnapshot);
		
		m_BannerSnapshots.Clear();
		
		DataSnapshot bannersSnapshot = applicationSnapshot.Child("banners");
		if (bannersSnapshot.Exists)
		{
			foreach (DataSnapshot bannerSnapshot in bannersSnapshot.Children)
				m_BannerSnapshots.Add(new BannerSnapshot(bannerSnapshot));
		}
	}

	BannerSnapshot GetBannerSnapshot(string _BannerID)
	{
		if (string.IsNullOrEmpty(_BannerID))
		{
			Debug.LogError("[ApplicationProcessor] Get banner snapshot failed. Banner ID is null or empty.");
			return null;
		}
		
		return m_BannerSnapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _BannerID);
	}
}