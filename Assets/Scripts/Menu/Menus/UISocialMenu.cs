using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.SocialMenu)]
public class UISocialMenu : UISlideMenu
{
	[SerializeField] GameObject m_AppleSignIn;
	[SerializeField] GameObject m_GoogleSignIn;
	[SerializeField] GameObject m_FacebookSignIn;

	[Inject] SocialProcessor m_SocialProcessor;
	[Inject] MenuProcessor   m_MenuProcessor;

	public void SignInApple() => SignIn(m_SocialProcessor.AttachAppleID);

	public void SignInGoogle() => SignIn(m_SocialProcessor.AttachGoogleID);

	public void SignInFacebook() => SignIn(m_SocialProcessor.AttachFacebookID);

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		#if UNITY_EDITOR
		m_AppleSignIn.SetActive(true);
		m_GoogleSignIn.SetActive(true);
		m_FacebookSignIn.SetActive(true);
		#elif UNITY_IOS
		m_AppleSignIn.SetActive(true);
		m_GoogleSignIn.SetActive(true);
		m_FacebookSignIn.SetActive(true);
		#elif UNITY_ANDROID
		m_AppleSignIn.SetActive(false);
		m_GoogleSignIn.SetActive(true);
		m_FacebookSignIn.SetActive(true);
		#endif
	}

	protected override bool OnEscape()
	{
		Hide();
		
		return true;
	}

	async void SignIn(Func<Task<bool>> _SignInTask)
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
			await m_MenuProcessor.ErrorAsync("login");
			
			await m_MenuProcessor.Show(MenuType.ErrorMenu, true);
			
			await m_MenuProcessor.Hide(MenuType.LoginMenu);
			
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		}
	}
}
