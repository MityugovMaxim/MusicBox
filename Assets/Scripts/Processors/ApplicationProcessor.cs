using System;
using System.IO;
using System.Threading.Tasks;
using AudioBox.Logging;
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

[Preserve]
public class ApplicationProcessor
{
	const string CACHE_VERSION_KEY = "CACHE_VERSION";

	public string ClientVersion => Application.version ?? string.Empty;
	public string ServerVersion => m_Snapshot?.Version ?? string.Empty;

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
		string cacheVersion  = CacheVersion;
		string clientVersion = ClientVersion;
		
		if (cacheVersion == clientVersion)
			return;
		
		Log.Info(
			this,
			"Clear cache. Cache version: '{0}' Client version: '{1}'.",
			cacheVersion,
			clientVersion
		);
		
		try
		{
			string[] directories = Directory.GetDirectories(Application.persistentDataPath);
			
			foreach (string directory in directories)
				Directory.Delete(directory, true);
			
			CacheVersion = ClientVersion;
			
			Log.Info(this, "Clear cache success. Cache version: '{0}' Client version: '{1}'.", CacheVersion, ClientVersion);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Clear cache failed. Cache version: '{0}' Client version: '{1}'.", CacheVersion, ClientVersion);
		}
	}
}