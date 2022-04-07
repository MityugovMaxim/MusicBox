using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class LanguageSnapshot
{
	public string ID     { get; }
	public string Name   { get; }
	public bool   Active { get; }
	public int    Order  { get; }

	public LanguageSnapshot(DataSnapshot _Data)
	{
		ID    = _Data.Key;
		Active = _Data.GetBool("active");
		Name  = _Data.GetString("name");
		Order = _Data.GetInt("order");
	}
}

[Preserve]
public class LanguageDataUpdateSignal { }

[Preserve]
public class LanguageSelectSignal { }

[Preserve]
public class LanguageProcessor
{
	public string Language { get; private set; }

	const string LANGUAGE_KEY = "LANGUAGE";

	bool Loaded { get; set; }

	[Inject] SignalBus             m_SignalBus;
	[Inject] LocalizationProcessor m_LocalizationProcessor;

	readonly List<LanguageSnapshot> m_Snapshots = new List<LanguageSnapshot>();

	DatabaseReference m_Data;

	public async Task Load()
	{
		if (m_Data == null)
		{
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child("languages");
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		Language = Determine();
		
		await m_LocalizationProcessor.Load(Language);
		
		Loaded = true;
	}

	public List<string> GetLanguages()
	{
		return m_Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetName(string _Language)
	{
		LanguageSnapshot snapshot = GetSnapshot(_Language);
		
		return snapshot?.Name ?? _Language;
	}

	public async Task<bool> Select(string _Language)
	{
		if (Language == _Language)
			return false;
		
		LanguageSnapshot snapshot = GetSnapshot(_Language);
		
		if (snapshot == null)
			return false;
		
		PlayerPrefs.SetString(LANGUAGE_KEY, snapshot.ID);
		
		Language = snapshot.ID;
		
		await m_LocalizationProcessor.Load(Language);
		
		m_SignalBus.Fire<LanguageSelectSignal>();
		
		return true;
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[LanguageProcessor] Updating languages data...");
		
		await Fetch();
		
		string language = Determine();
		
		await Select(language);
		
		Debug.Log("[LanguageProcessor] Update languages data complete.");
		
		m_SignalBus.Fire<LanguageDataUpdateSignal>();
		
		Determine();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.OrderByChild("order").GetValueAsync(15000, 4);
		
		if (dataSnapshot == null)
		{
			Debug.LogError("[LanguageProcessor] Fetch languages failed.");
			return;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(_Data => new LanguageSnapshot(_Data)));
	}

	string Determine()
	{
		string language = PlayerPrefs.HasKey(LANGUAGE_KEY)
			? PlayerPrefs.GetString(LANGUAGE_KEY)
			: Application.systemLanguage.GetCode();
		
		LanguageSnapshot snapshot = GetSnapshot(language);
		if (snapshot != null)
			return snapshot.ID;
		
		LanguageSnapshot fallback = GetSnapshot(SystemLanguage.English.GetCode());
		if (fallback != null)
			return fallback.ID;
		
		LanguageSnapshot supported = m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot != null);
		if (supported != null)
			return supported.ID;
		
		Debug.LogError("[LanguageProcessor] Determine language failed.");
		
		return null;
	}

	LanguageSnapshot GetSnapshot(string _Language)
	{
		if (m_Snapshots.Count == 0)
			return null;
		
		if (string.IsNullOrEmpty(_Language))
		{
			Debug.LogError("[LanguagesProcessor] Get snapshot failed. Language is null or empty.");
			return null;
		}
		
		LanguageSnapshot snapshot = m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _Language);
		
		if (snapshot == null)
			Debug.LogErrorFormat("[LanguagesProcessor] Get snapshot failed. Snapshot with language '{0}' is null.", _Language);
		
		return snapshot;
	}
}