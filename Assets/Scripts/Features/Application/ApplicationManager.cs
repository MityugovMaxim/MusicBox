using System;
using System.IO;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ApplicationManager : DataObject<ApplicationObject>, IDataObject
{
	protected override string Path => "application";

	const string CACHE_VERSION_KEY = "CACHE_VERSION";

	public string ClientVersion => Application.version ?? string.Empty;
	public string ServerVersion => m_Object?.Version ?? string.Empty;

	static string CacheVersion
	{
		get => PlayerPrefs.GetString(CACHE_VERSION_KEY, string.Empty);
		set => PlayerPrefs.SetString(CACHE_VERSION_KEY, value);
	}

	DatabaseReference m_Data;

	ApplicationObject m_Object;

	public async Task Load()
	{
		if (m_Data == null)
			m_Data = FirebaseDatabase.DefaultInstance.RootReference.Child("application");
		
		await Fetch();
		
		TryClearCache();
	}

	public string GetReviewURL()
	{
		#if UNITY_IOS
		return m_Object?.AppStoreReviewURL;
		#elif UNITY_ANDROID
		return m_Object?.GooglePlayReviewURL;
		#endif
	}

	protected override ApplicationObject Create(DataSnapshot _Data) => new ApplicationObject(_Data);

	async Task Fetch()
	{
		DataSnapshot dataSnapshot = await m_Data.GetValueAsync(15000, 4);
		
		if (dataSnapshot == null)
		{
			Debug.LogError("[ApplicationProcessor] Fetch application failed.");
			return;
		}
		
		m_Object = new ApplicationObject(dataSnapshot);
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
