using System;
using System.Collections.Generic;
using AudioBox.Logging;
using TMPro;
using UnityEngine;
using Zenject;

[Menu(MenuType.LocalizationSettingsMenu)]
public class UILocalizationSettingsMenu : UIMenu
{
	string Key   => m_KeyField.text;
	string Value => m_ValueField.text;

	[SerializeField] TMP_InputField m_KeyField;
	[SerializeField] TMP_InputField m_ValueField;
	[SerializeField] TMP_Text       m_ResultLabel;

	[Inject] LocalizationProcessor m_LocalizationProcessor;
	[Inject] MenuProcessor         m_MenuProcessor;

	string m_Key;
	string m_Value;

	public void Setup(string _Key, string _Value)
	{
		m_Key   = _Key;
		m_Value = _Value;
		
		m_KeyField.text   = m_Key;
		m_ValueField.text = m_Value;
		
		m_KeyField.onSubmit.RemoveAllListeners();
		m_KeyField.onSubmit.AddListener(ProcessKey);
		
		m_ValueField.onValueChanged.RemoveAllListeners();
		m_ValueField.onValueChanged.AddListener(ProcessValue);
		
		ProcessValue(m_Value);
	}

	public async void Back()
	{
		await m_MenuProcessor.Show(MenuType.LocalizationMenu, true);
		await m_MenuProcessor.Hide(MenuType.LocalizationSettingsMenu);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_LocalizationProcessor.Restore();
		
		Dictionary<string, string> localization = m_LocalizationProcessor.GetLocalization();
		
		if (localization.ContainsKey(m_Key))
			m_Value = localization[m_Key];
		else
			localization[m_Key] = m_Value;
		
		m_LocalizationProcessor.SetLocalization(localization);
		
		Setup(m_Key, m_Value);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Upload()
	{
		Dictionary<string, string> localization = m_LocalizationProcessor.GetLocalization();
		
		localization.Remove(m_Key);
		
		localization[Key] = Value;
		
		m_LocalizationProcessor.SetLocalization(localization);
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_LocalizationProcessor.Upload();
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload localization failed");
			
			string message = exception.GetBaseException().Message;
			
			await m_MenuProcessor.ErrorAsync(
				"upload_localization",
				"Upload failed",
				message
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	protected override void OnShowFinished()
	{
		Dictionary<string, string> localization = m_LocalizationProcessor.GetLocalization();
		
		localization.Remove(m_Key);
		
		localization[Key] = Value;
		
		m_LocalizationProcessor.SetLocalization(localization);
	}

	void ProcessKey(string _Key)
	{
		m_KeyField.text = _Key.ToAllCapital();
	}

	void ProcessValue(string _Value)
	{
		m_ResultLabel.text = _Value;
	}
}