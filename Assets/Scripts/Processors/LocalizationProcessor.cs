using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Storage;
using UnityEngine;
using Zenject;

public class LocalizationSignal { }

public class LocalizationProcessor
{
	readonly Dictionary<string, string> m_Localization = new Dictionary<string, string>();

	readonly SignalBus        m_SignalBus;
	readonly StorageProcessor m_StorageProcessor;

	StorageReference m_LocalizationReference;

	public LocalizationProcessor(
		SignalBus        _SignalBus,
		StorageProcessor _StorageProcessor
	)
	{
		m_SignalBus        = _SignalBus;
		m_StorageProcessor = _StorageProcessor;
	}

	public async Task LoadLocalization()
	{
		if (m_LocalizationReference == null)
		{
			string language = Application.systemLanguage.ToString();
			m_LocalizationReference = FirebaseStorage.DefaultInstance.RootReference.Child($"Localization/{language}.json");
		}
		
		
	}

	async Task FetchLocalization()
	{
		m_SignalBus.Fire<LocalizationSignal>();
	}
}