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

[Menu(MenuType.LocalizationMenu)]
public class UILocalizationMenu : UIMenu
{
	const float LIST_SPACING = 15;

	[SerializeField] UILayout m_Content;
	[SerializeField] Button   m_BackButton;
	[SerializeField] Button   m_RestoreButton;
	[SerializeField] Button   m_UploadButton;
	[SerializeField] Button   m_AddButton;
	[SerializeField] Button   m_SortButton;

	[Inject] StorageProcessor           m_StorageProcessor;
	[Inject] MenuProcessor              m_MenuProcessor;
	[Inject] LanguageProcessor          m_LanguageProcessor;
	[Inject] UILocalizationElement.Pool m_Pool;

	LocalizationData m_Localization;

	protected override void Awake()
	{
		base.Awake();
		
		m_BackButton.onClick.AddListener(Back);
		m_RestoreButton.onClick.AddListener(Restore);
		m_UploadButton.onClick.AddListener(Upload);
		m_AddButton.onClick.AddListener(Add);
		m_SortButton.onClick.AddListener(Sort);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_BackButton.onClick.RemoveListener(Back);
		m_RestoreButton.onClick.RemoveListener(Restore);
		m_UploadButton.onClick.RemoveListener(Upload);
		m_AddButton.onClick.RemoveListener(Add);
		m_SortButton.onClick.RemoveListener(Sort);
	}

	public void Setup(LocalizationData _Localization)
	{
		m_Localization = _Localization;
	}

	public void Reorder(int _SourceIndex, int _TargetIndex)
	{
		if (m_Localization != null)
			m_Localization.Reorder(_SourceIndex, _TargetIndex);
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		if (m_Localization != null)
			m_Localization.OnChanged += Refresh;
		
		Refresh();
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		
		if (m_Localization != null)
			m_Localization.OnChanged -= Refresh;
	}

	void Back()
	{
		Hide();
	}

	async void Restore()
	{
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"restore",
			$"RESTORE {m_Localization.Language}",
			"Are you sure want to restore localization?"
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
			$"UPLOAD {m_Localization.Language}",
			"Are you sure want to upload localization?"
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
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	void Add()
	{
		m_Localization.Create();
		
		Refresh();
	}

	void Sort()
	{
		m_Localization.Sort();
		
		Refresh();
	}

	void Refresh()
	{
		m_Content.Clear();
		
		VerticalStackLayout.Start(m_Content, LIST_SPACING);
		
		foreach (LocalizationData.Entry entry in m_Localization.Entries)
		{
			if (entry == null)
				continue;
			
			if (string.IsNullOrEmpty(entry.Key))
				continue;
			
			LocalizationElementEntity item = new LocalizationElementEntity(
				entry.Key,
				m_Localization,
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
		if (m_Localization == null)
			return;
		
		string language = m_Localization.Language;
		
		if (string.IsNullOrEmpty(language))
			return;
		
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
		
		m_Localization.Rebuild(localization);
	}

	async Task UploadAsync()
	{
		if (m_Localization == null)
			return;
		
		string language = m_Localization.Language;
		
		if (string.IsNullOrEmpty(language))
			return;
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference
			.Child("Localization")
			.Child($"{language}.lang");
		
		string json = m_Localization.Serialize();
		
		byte[] bytes = Encoding.Unicode.GetBytes(json);
		
		byte[] encode = Compression.Compress(bytes);
		
		await reference.PutBytesAsync(encode);
		
		await m_LanguageProcessor.Reload();
	}
}