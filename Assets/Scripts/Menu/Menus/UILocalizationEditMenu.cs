using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AudioBox.Compression;
using Firebase.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing.MiniJSON;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.LocalizationEditMenu)]
public class UILocalizationEditMenu : UIMenu
{
	[SerializeField] UILocalizationField m_KeyField;
	[SerializeField] UILocalizationField m_ValueField;
	[SerializeField] TMP_Text            m_PreviewLabel;
	[SerializeField] Button              m_BackButton;
	[SerializeField] Button              m_RestoreButton;
	[SerializeField] Button              m_UploadButton;

	[Inject] StorageProcessor  m_StorageProcessor;
	[Inject] LanguageProcessor m_LanguageProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;

	string           m_Key;
	LocalizationData m_Localization;

	string m_CachedKey;
	string m_CachedValue;

	protected override void Awake()
	{
		base.Awake();
		
		m_KeyField.Submit.AddListener(SubmitKey);
		m_ValueField.Submit.AddListener(SubmitValue);
		m_BackButton.onClick.AddListener(Back);
		m_RestoreButton.onClick.AddListener(Restore);
		m_UploadButton.onClick.AddListener(Upload);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_KeyField.Submit.RemoveListener(SubmitKey);
		m_ValueField.Submit.RemoveListener(SubmitValue);
		m_BackButton.onClick.RemoveListener(Back);
		m_RestoreButton.onClick.RemoveListener(Restore);
		m_UploadButton.onClick.RemoveListener(Upload);
	}

	public void Setup(string _Key, LocalizationData _Localization)
	{
		m_Key          = _Key;
		m_Localization = _Localization;
		
		m_CachedKey   = m_Key;
		m_CachedValue = m_Localization.GetValue(m_Key);
		
		m_KeyField.SetValue(m_Key);
		m_ValueField.SetValue(m_Localization.GetValue(m_Key));
		m_PreviewLabel.text = m_Localization.GetValue(m_Key);
	}

	void Back()
	{
		Hide();
	}

	async void Restore()
	{
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"restore",
			$"RESTORE {m_Key}",
			"Are you sure want to restore key and value?"
		);
		
		if (!confirm)
			return;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		if (m_Key != m_CachedKey)
		{
			m_Localization.Rename(m_Key, m_CachedKey);
			
			m_Key = m_CachedKey;
		}
		
		string value = m_Localization.GetValue(m_Key);
		
		if (value != m_CachedValue)
		{
			m_Localization.SetValue(m_Key, m_CachedValue);
		}
		
		m_KeyField.SetValue(m_Key);
		m_ValueField.SetValue(m_Localization.GetValue(m_Key));
		m_PreviewLabel.text = m_Localization.GetValue(m_Key);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	async void Upload()
	{
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"upload",
			$"UPLOAD {m_Key}",
			"Are you sure want to upload key and value?"
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

	async Task<LocalizationData> LoadAsync()
	{
		if (m_Localization == null)
			return null;
		
		string language = m_Localization.Language;
		
		if (string.IsNullOrEmpty(language))
			return null;
		
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
		
		return new LocalizationData(m_Localization.Language, localization);
	}

	async Task UploadAsync()
	{
		LocalizationData localization = await LoadAsync();
		
		if (localization == null)
			return;
		
		string language = localization.Language;
		
		if (string.IsNullOrEmpty(language))
			return;
		
		localization.SetValue(m_Key, m_Localization.GetValue(m_Key));
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference
			.Child("Localization")
			.Child($"{language}.lang");
		
		string json = localization.Serialize();
		
		byte[] bytes = Encoding.Unicode.GetBytes(json);
		
		byte[] encode = Compression.Compress(bytes);
		
		await reference.PutBytesAsync(encode);
		
		await m_LanguageProcessor.Reload();
	}

	void SubmitKey(string _Key)
	{
		string key = _Key.ToAllCapital();
		
		m_Localization.Rename(m_Key, key);
		
		m_Key = key;
	}

	void SubmitValue(string _Value)
	{
		m_Localization.SetValue(m_Key, _Value);
		
		m_ValueField.SetValue(m_Localization.GetValue(m_Key));
		m_PreviewLabel.text = m_Localization.GetValue(m_Key);
	}
}