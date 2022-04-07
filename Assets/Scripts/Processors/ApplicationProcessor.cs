using System;
using System.IO;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
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
	public string Image     { get; }
	public string Language  { get; }
	public bool   Permanent { get; }
	public string Version   { get; }
	public string URL       { get; }

	public BannerSnapshot(DataSnapshot _Data)
	{
		ID        = _Data.Key;
		Active    = _Data.GetBool("active");
		Language  = _Data.GetString("language");
		Image     = _Data.GetString("image");
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

	[Inject] SignalBus m_SignalBus;

	DatabaseReference m_Data;

	ApplicationSnapshot m_Snapshot;

	public async Task Load()
	{
		if (m_Data == null)
		{
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child("application");
			m_Data.ValueChanged += OnUpdate;
		}
		
		await FacebookAuth.Initialize();
		
		await Fetch();
		
		TryClearCache();
		
		Loaded = true;
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[ApplicationProcessor] Updating application data...");
		
		await Fetch();
		
		Debug.Log("[ApplicationProcessor] Update application data complete.");
		
		m_SignalBus.Fire<ApplicationDataUpdateSignal>();
	}

	async Task Fetch()
	{
		DataSnapshot dataSnapshot = await m_Data.GetValueAsync(15000, 4);
		
		if (dataSnapshot == null)
		{
			Debug.LogError("[ApplicationProcessor] Fetch application failed.");
			return;
		}
		
		m_Snapshot = new ApplicationSnapshot(dataSnapshot);
	}

	void TryClearCache()
	{
		(int sourceMajor, int sourceMinor, int sourcePatch) = GetVersion(CacheVersion);
		(int targetMajor, int targetMinor, int targetPatch) = GetVersion(m_Snapshot.Version);
		
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
			
			CacheVersion = m_Snapshot.Version;
			
			Debug.LogError("[ApplicationProcessor] Clear cache success.");
		}
		catch (Exception exception)
		{
			Debug.LogError("[ApplicationProcessor] Clear cache failed.");
			Debug.LogException(exception);
		}
	}

	static (int Major, int Minor, int Patch) GetVersion(string _Version)
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
}