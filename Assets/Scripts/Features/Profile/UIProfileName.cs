using System;
using TMPro;
using UnityEngine;
using Zenject;

public class UIProfileName : UIEntity
{
	[SerializeField] TMP_Text m_Username;

	[Inject] SocialProcessor m_SocialProcessor;
	[Inject] Localization    m_Localization;
	[Inject] MenuProcessor   m_MenuProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessUsername();
		
		m_SocialProcessor.OnLogin      += ProcessUsername;
		m_SocialProcessor.OnLogout     += ProcessUsername;
		m_SocialProcessor.OnNameChange += ProcessUsername;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_SocialProcessor.OnLogin      -= ProcessUsername;
		m_SocialProcessor.OnLogout     -= ProcessUsername;
		m_SocialProcessor.OnNameChange -= ProcessUsername;
	}

	public async void SetUsername(string _Username)
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_SocialProcessor.SetUsername(_Username);
			
			m_HapticProcessor.Process(Haptic.Type.Success);
		}
		catch (Exception exception)
		{
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	void ProcessUsername()
	{
		m_Username.text = GetUsername();
	}

	string GetUsername()
	{
		string username = m_SocialProcessor.Name;
		if (!string.IsNullOrEmpty(username))
			return username;
		
		string email = m_SocialProcessor.Email;
		if (!string.IsNullOrEmpty(email))
			return email.Split('@')[0];
		
		string device = SystemInfo.deviceName;
		if (!string.IsNullOrEmpty(device))
			return device;
		
		return m_SocialProcessor.Guest
			? m_Localization.Get("COMMON_GUEST")
			: SystemInfo.deviceModel;
	}
}
