using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class LanguagesManager : IDataCollection
{
	const string LANGUAGE_KEY = "LANGUAGE";

	public DataPriority Priority => DataPriority.High;

	public bool Loaded => Collection.Loaded && Localization.Loaded;

	public LanguagesCollection Collection   => m_LanguagesCollection;
	public Localization        Localization => m_Localization;

	public string Language { get; private set; }

	public DynamicDelegate<string> OnLanguageChange { get; } = new DynamicDelegate<string>();

	[Inject] LanguagesCollection m_LanguagesCollection;
	[Inject] Localization        m_Localization;

	Task m_Loading;

	public async Task Load()
	{
		await Collection.Load();
		
		Determine();
		
		await Localization.Load(Language);
		
		Log.Info(
			this,
			"System language: {0}. Client language: {1}.",
			Application.systemLanguage.GetCode(),
			Language
		);
	}

	public async Task Reload()
	{
		await Collection.Reload();
		
		Determine();
		
		await Localization.Reload(Language);
	}

	public void Unload()
	{
		Collection.Unload();
		
		Localization.Unload();
	}

	public List<string> GetLanguages()
	{
		return m_LanguagesCollection.GetIDs()
			.Where(IsActive)
			.ToList();
	}

	public string GetName(string _Language)
	{
		LanguageSnapshot snapshot = Collection.GetSnapshot(_Language);
		
		return snapshot?.Name ?? _Language;
	}

	public async Task Select(string _Language)
	{
		if (Language == _Language)
			return;
		
		LanguageSnapshot snapshot = m_LanguagesCollection.GetSnapshot(_Language);
		
		if (snapshot == null)
			return;
		
		PlayerPrefs.SetString(LANGUAGE_KEY, snapshot.ID);
		
		Language = snapshot.ID;
		
		await m_Localization.Load(Language);
		
		OnLanguageChange?.Invoke(Language);
	}

	void Determine()
	{
		string language = PlayerPrefs.HasKey(LANGUAGE_KEY)
			? PlayerPrefs.GetString(LANGUAGE_KEY)
			: Application.systemLanguage.GetCode();
		
		LanguageSnapshot snapshot = m_LanguagesCollection.GetSnapshot(language);
		if (snapshot != null)
		{
			Language = snapshot.ID;
			return;
		}
		
		LanguageSnapshot fallback = m_LanguagesCollection.GetSnapshot(SystemLanguage.English.GetCode());
		if (fallback != null)
		{
			Language = fallback.ID;
			return;
		}
		
		LanguageSnapshot supported = m_LanguagesCollection.Snapshots.FirstOrDefault(_Snapshot => _Snapshot != null);
		if (supported != null)
		{
			Language = supported.ID;
			return;
		}
		
		Log.Error(this, "Determine language failed.");
		
		Language = SystemLanguage.English.GetCode();
	}

	bool IsActive(string _Language)
	{
		LanguageSnapshot snapshot = m_LanguagesCollection.GetSnapshot(_Language);
		
		return snapshot?.Active ?? false;
	}
}
