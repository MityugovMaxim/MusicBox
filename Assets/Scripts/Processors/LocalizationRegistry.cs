using System;
using System.Collections.Generic;
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
		
		LocalizationRegistry localization = Resources.Load<LocalizationRegistry>($"Localization/{language}");
		
		if (localization == null)
			localization = Resources.Load<LocalizationRegistry>($"Localization{SystemLanguage.English.GetCode()}");
		
		if (localization == null)
			return;
		
		foreach (LocalizationEntry entry in localization.m_Entries)
			_Localization[entry.Key] = entry.Value;
		
		Resources.UnloadAsset(localization);
	}
}