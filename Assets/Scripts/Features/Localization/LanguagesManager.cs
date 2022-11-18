using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

public class LanguagesManager : IInitializable
{
	const string LANGUAGE_KEY = "LANGUAGE";

	public DynamicDelegate<string> OnLanguageChange;

	public string Language { get; private set; }

	[Inject] LanguagesCollection   m_Languages;
	[Inject] Localization m_Localization;

	async void IInitializable.Initialize()
	{
		await m_Languages.Load();
		
		Detect();
	}

	public List<string> GetLanguages()
	{
		return m_Languages.GetIDs()
			.Where(IsActive)
			.ToList();
	}

	public async Task Select(string _Language)
	{
		if (Language == _Language)
			return;
		
		LanguageSnapshot snapshot = m_Languages.GetSnapshot(_Language);
		
		if (snapshot == null)
			return;
		
		PlayerPrefs.SetString(LANGUAGE_KEY, snapshot.ID);
		
		Language = snapshot.ID;
		
		await m_Localization.Load(Language);
		
		m_Localization.LoadBuiltIn();
		
		OnLanguageChange?.Invoke(Language);
	}

	void Detect()
	{
		string language = PlayerPrefs.HasKey(LANGUAGE_KEY)
			? PlayerPrefs.GetString(LANGUAGE_KEY)
			: Application.systemLanguage.GetCode();
		
		LanguageSnapshot snapshot = m_Languages.GetSnapshot(language);
		if (snapshot != null)
		{
			Language = snapshot.ID;
			return;
		}
		
		LanguageSnapshot fallback = m_Languages.GetSnapshot(SystemLanguage.English.GetCode());
		if (fallback != null)
		{
			Language = fallback.ID;
			return;
		}
		
		LanguageSnapshot supported = m_Languages.Snapshots.FirstOrDefault(_Snapshot => _Snapshot != null);
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
		LanguageSnapshot snapshot = m_Languages.GetSnapshot(_Language);
		
		return snapshot?.Active ?? false;
	}
}
