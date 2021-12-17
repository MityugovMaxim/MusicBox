using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Storage;
using UnityEngine;
using Zenject;

public class LanguageProcessor
{
	readonly Dictionary<string, string> m_Localization = new Dictionary<string, string>();

	readonly StorageProcessor m_StorageProcessor;

	StorageReference m_LocalizationReference;

	[Inject]
	public LanguageProcessor(StorageProcessor _StorageProcessor)
	{
		m_StorageProcessor = _StorageProcessor;
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
		m_Localization.Clear();
		
		Dictionary<string, string> localization = await m_StorageProcessor.LoadLocalization(Application.systemLanguage);
		
		foreach (var entry in localization)
			m_Localization[entry.Key] = entry.Value;
	}
}