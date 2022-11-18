using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AudioBox.Compression;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.Purchasing.MiniJSON;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.LanguagesMenu)]
public class UILanguagesMenu : UIMenu
{
	const float LIST_SPACING = 15;

	[SerializeField] UILayout m_Content;
	[SerializeField] UIGroup  m_LoaderGroup;
	[SerializeField] Button   m_BackButton;
	[SerializeField] Button   m_RestoreButton;
	[SerializeField] Button   m_UploadButton;
	[SerializeField] Button   m_SyncButton;

	[Inject] UILanguageElement.Pool m_Pool;
	[Inject] LanguagesCollection      m_LanguagesCollection;
	[Inject] StorageProcessor       m_StorageProcessor;
	[Inject] MenuProcessor          m_MenuProcessor;

	readonly List<LocalizationData> m_Localizations = new List<LocalizationData>();

	protected override void Awake()
	{
		base.Awake();
		
		m_BackButton.onClick.AddListener(Back);
		m_RestoreButton.onClick.AddListener(Restore);
		m_UploadButton.onClick.AddListener(Upload);
		m_SyncButton.onClick.AddListener(Sync);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_BackButton.onClick.RemoveListener(Back);
		m_RestoreButton.onClick.RemoveListener(Restore);
		m_UploadButton.onClick.RemoveListener(Upload);
		m_SyncButton.onClick.RemoveListener(Sync);
	}

	void Back()
	{
		Hide();
	}

	async void Restore()
	{
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"upload",
			"RESTORE",
			"Are you sure want to restore localizations?"
		);
		
		if (!confirm)
			return;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await LoadAsync();
		}
		catch (Exception exception)
		{
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	async void Upload()
	{
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"upload",
			"UPLOAD",
			"Are you sure want to upload localizations?"
		);
		
		if (!confirm)
			return;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await UploadAsync();
		}
		catch (Exception exception)
		{
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	async void Sync()
	{
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"sync",
			"SYNC",
			"Are you sure want to sync keys for all languages?"
		);
		
		if (!confirm)
			return;
		
		HashSet<string> keys = new HashSet<string>();
		
		foreach (LocalizationData item in m_Localizations)
		foreach (string key in item.Localization.Keys)
		{
			keys.Add(key);
		}
		
		foreach (string key in keys)
		foreach (LocalizationData item in m_Localizations)
			item.Add(key, string.Empty);
	}

	protected override async void OnShowStarted()
	{
		base.OnShowStarted();
		
		await LoadAsync();
		
		Refresh();
	}

	void Refresh()
	{
		m_Content.Clear();
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (LocalizationData localization in m_Localizations)
		{
			if (localization == null)
				continue;
			
			LanguageElementEntity item = new LanguageElementEntity(
				localization,
				m_Pool
			);
			
			m_Content.Add(item);
		}
		
		VerticalStackLayout.End(m_Content);
		
		m_Content.Space(LIST_SPACING);
		
		m_Content.Reposition();
	}

	async Task LoadAsync()
	{
		m_LoaderGroup.Show();
		
		m_Localizations.Clear();
		
		List<string> languages = m_LanguagesCollection.GetLanguages(true);
		
		foreach (string language in languages)
		{
			string json = await m_StorageProcessor.LoadJson(
				$"Localization/{language}.lang",
				true,
				Encoding.Unicode,
				null
			);
			
			Dictionary<string, string> localization = new Dictionary<string, string>();
			
			if (Json.Deserialize(json) is Dictionary<string, object> data)
			{
				foreach (string key in data.Keys)
					localization[key] = data.GetString(key);
			}
			
			LocalizationData item = new LocalizationData(language, localization);
			
			m_Localizations.Add(item);
		}
		
		m_LoaderGroup.Hide();
	}

	async Task UploadAsync()
	{
		if (m_Localizations == null || m_Localizations.Count == 0)
			return;
		
		foreach (LocalizationData item in m_Localizations)
		{
			if (item == null)
				continue;
			
			StorageReference reference = FirebaseStorage.DefaultInstance.RootReference
				.Child("Localization")
				.Child($"{item.Language}.lang");
			
			string json = item.Serialize();
			
			byte[] bytes = Encoding.Unicode.GetBytes(json);
			
			byte[] encode = Compression.Compress(bytes);
			
			await reference.PutBytesAsync(encode);
		}
		
		await m_LanguagesCollection.Reload();
	}
}
