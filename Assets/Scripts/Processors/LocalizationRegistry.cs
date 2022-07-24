using System;
using System.Collections.Generic;
using AudioBox.Logging;
using UnityEngine;

[CreateAssetMenu(menuName = "Localization Registry", fileName = "Localization Registry")]
public class LocalizationRegistry : ScriptableObject
{
	const string LANGUAGE_KEY = "LANGUAGE";

	[Serializable]
	public class LocalizationEntry
	{
		public string Key   => m_Key;
		public string Value => m_Value;

		[SerializeField]            string m_Key;
		[SerializeField, Multiline] string m_Value;
	}

	[SerializeField] List<LocalizationEntry> m_Entries;

	public static void Load(Dictionary<string, string> _Localization)
	{
		string language = PlayerPrefs.HasKey(LANGUAGE_KEY)
			? PlayerPrefs.GetString(LANGUAGE_KEY)
			: Application.systemLanguage.GetCode();
		
		LocalizationRegistry[] localizations = Resources.LoadAll<LocalizationRegistry>($"{language}");
		
		if (localizations == null || localizations.Length == 0)
			localizations = Resources.LoadAll<LocalizationRegistry>($"{SystemLanguage.English.GetCode()}");
		
		if (localizations == null || localizations.Length == 0)
		{
			Log.Error(typeof(LocalizationRegistry), "Load built-in localization failed.");
			return;
		}
		
		foreach (LocalizationRegistry localization in localizations)
		foreach (LocalizationEntry entry in localization.m_Entries)
			_Localization[entry.Key] = entry.Value;
		
		foreach (LocalizationRegistry localization in localizations)
			Resources.UnloadAsset(localization);
	}
}