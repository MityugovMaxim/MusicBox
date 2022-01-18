using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class LanguageProcessor
{
	const string LANGUAGE_KEY = "LANGUAGE";

	public string Language { get; private set; }

	public static IReadOnlyCollection<string> SupportedLanguages => m_Languages;

	readonly StorageProcessor m_StorageProcessor;

	static readonly string[] m_Languages =
	{
		SystemLanguage.English.GetCode(),
		SystemLanguage.Russian.GetCode(),
		SystemLanguage.German.GetCode(),
		SystemLanguage.Spanish.GetCode(),
		SystemLanguage.Portuguese.GetCode(),
	};

	readonly Dictionary<string, string> m_Localization = new Dictionary<string, string>();

	[Inject]
	public LanguageProcessor(StorageProcessor _StorageProcessor)
	{
		m_StorageProcessor = _StorageProcessor;
	}

	public void SetLanguage(string _Language)
	{
		if (!m_Languages.Contains(_Language))
			return;
		
		PlayerPrefs.SetString(LANGUAGE_KEY, _Language);
		
		Language = _Language;
	}

	public bool SupportsLanguage(string _Language)
	{
		return string.IsNullOrEmpty(_Language) || Language == _Language || Application.systemLanguage.GetCode() == _Language;
	}

	public string Get(string _Key)
	{
		if (m_Localization.TryGetValue(_Key, out string text) && !string.IsNullOrEmpty(text))
			return text;
		
		return $"MISSING KEY: {_Key}";
	}

	public string Format(string _Key, params object[] _Args)
	{
		if (m_Localization.TryGetValue(_Key, out string mask) && !string.IsNullOrEmpty(mask))
			return string.Format(mask, _Args);
		
		return $"MISSING KEY: {_Key}";
	}

	public async Task LoadLocalization()
	{
		Language = LoadLanguage();
		
		m_Localization.Clear();
		
		Dictionary<string, string> localization = await m_StorageProcessor.LoadLocalization(Language);
		
		foreach (var entry in localization)
			m_Localization[entry.Key] = entry.Value;
	}

	static string LoadLanguage()
	{
		string language = PlayerPrefs.HasKey(LANGUAGE_KEY) ? PlayerPrefs.GetString(LANGUAGE_KEY) : Application.systemLanguage.GetCode();
		
		return m_Languages.Contains(language) ? language : SystemLanguage.English.GetCode();
	}
}