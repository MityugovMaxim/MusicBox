using System;
using System.Collections.Generic;
using System.IO;
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
	const string CACHE_VERSION_KEY = "CACHE_VERSION";

	static string CacheVersion
	{
		get => PlayerPrefs.GetString(CACHE_VERSION_KEY, string.Empty);
		set => PlayerPrefs.SetString(CACHE_VERSION_KEY, value);
	}

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
		
		TryClearCache();
		
		Loaded = true;
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

	void TryClearCache()
	{
		(int sourceMajor, int sourceMinor, int sourcePatch) = GetVersion(CacheVersion);
		(int targetMajor, int targetMinor, int targetPatch) = GetVersion(m_ApplicationSnapshot.Version);
		
		if (sourceMajor != targetMajor || sourceMinor != targetMinor || sourcePatch != targetPatch)
			return;
		
		Debug.LogFormat(
			"[ApplicationProcessor] Clear cache. Cache version: '{0}.{1}.{2}' Application version: '{3}.{4}.{5}'.",
			sourceMajor, sourceMinor, sourcePatch,
			targetMajor, targetMinor, targetPatch
		);
		
		try
		{
			string[] directories = Directory.GetDirectories(Application.persistentDataPath);
			
			foreach (string directory in directories)
				Directory.Delete(directory, true);
			
			CacheVersion = m_ApplicationSnapshot.Version;
			
			Debug.LogError("[ApplicationProcessor] Clear cache success.");
		}
		catch (Exception exception)
		{
			Debug.LogError("[ApplicationProcessor] Clear cache failed.");
			Debug.LogException(exception);
		}
	}

	(int Major, int Minor, int Build) GetVersion(string _Version)
	{
		if (string.IsNullOrEmpty(_Version))
			return (0, 0, 0);
		
		string[] data = _Version.Split(new char[] { '.', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
		
		int major;
		if (data.Length >= 1 && !string.IsNullOrEmpty(data[0]))
			int.TryParse(data[0], out major);
		else
			major = 1;
		
		int minor;
		if (data.Length >= 2 && !string.IsNullOrEmpty(data[1]))
			int.TryParse(data[1], out minor);
		else
			minor = 0;
		
		int patch;
		if (data.Length >= 3 && !string.IsNullOrEmpty(data[2]))
			int.TryParse(data[1], out patch);
		else
			patch = 0;
		
		return (major, minor, patch);
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