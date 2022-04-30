using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

[Menu(MenuType.SocialMenu)]
public class UISocialMenu : UISlideMenu
{
	[SerializeField] TMP_InputField m_Email;
	[SerializeField] TMP_InputField m_Password;

	[Inject] SocialProcessor m_SocialProcessor;
	[Inject] MenuProcessor   m_MenuProcessor;

	public void SignInEmail()
	{
		SignIn(
			() => m_SocialProcessor.AttachEmail(m_Email.text, m_Password.text),
			"Sign in with Email failed",
			"Check your Internet connection and try to sign in with Email again"
		);
	}

	public void SignInApple()
	{
		SignIn(
			m_SocialProcessor.AttachAppleID,
			GetLocalization("APPLE_SIGN_IN_ERROR_TITLE"),
			GetLocalization("APPLE_SING_IN_ERROR_MESSAGE")
		);
	}

	public void SignInGoogle()
	{
		SignIn(
			m_SocialProcessor.AttachGoogleID,
			GetLocalization("GOOGLE_SIGN_IN_ERROR_TITLE"),
			GetLocalization("GOOGLE_SIGN_IN_ERROR_MESSAGE")
		);
	}

	public void SignInFacebook()
	{
		SignIn(
			m_SocialProcessor.AttachFacebookID,
			GetLocalization("FACEBOOK_SIGN_IN_ERROR_TITLE"),
			GetLocalization("FACEBOOK_SIGN_IN_ERROR_MESSAGE")
		);
	}

	protected override void OnShowStarted()
	{
		m_Email.text    = string.Empty;
		m_Password.text = string.Empty;
	}

	async void SignIn(Func<Task<bool>> _SignInTask, string _Title, string _Message)
	{
		if (_SignInTask == null)
			return;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.LoginMenu);
		
		bool success = await _SignInTask();
		
		if (success)
		{
			await m_MenuProcessor.Hide(MenuType.SocialMenu, true);
			
			await m_MenuProcessor.Hide(MenuType.MainMenu, true);
			
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
			
			UILoginMenu loginMenu = m_MenuProcessor.GetMenu<UILoginMenu>();
			if (loginMenu != null)
				await loginMenu.Login();
		}
		else
		{
			UIErrorMenu errorMenu = m_MenuProcessor.GetMenu<UIErrorMenu>();
			if (errorMenu != null)
				errorMenu.Setup("sign_in_error", _Title, _Message);
			
			await m_MenuProcessor.Show(MenuType.ErrorMenu, true);
			
			await m_MenuProcessor.Hide(MenuType.LoginMenu);
			
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		}
	}
}