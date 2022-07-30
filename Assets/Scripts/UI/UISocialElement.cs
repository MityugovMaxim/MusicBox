using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

public class UISocialElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UISocialElement> { }

	[SerializeField] Button m_AppleButton;
	[SerializeField] Button m_GoogleButton;
	[SerializeField] Button m_FacebookButton;

	[Inject] LocalizationProcessor m_LocalizationProcessor;
	[Inject] SocialProcessor       m_SocialProcessor;
	[Inject] MenuProcessor         m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		#if UNITY_IOS
		m_AppleButton.gameObject.SetActive(true);
		m_GoogleButton.gameObject.SetActive(true);
		m_FacebookButton.gameObject.SetActive(true);
		#elif UNITY_ANDROID
		m_AppleButton.gameObject.SetActive(false);
		m_GoogleButton.gameObject.SetActive(true);
		m_FacebookButton.gameObject.SetActive(true);
		#endif
		
		m_AppleButton.onClick.AddListener(SignInApple);
		m_GoogleButton.onClick.AddListener(SignInGoogle);
		m_FacebookButton.onClick.AddListener(SignInFacebook);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_AppleButton.onClick.RemoveListener(SignInApple);
		m_GoogleButton.onClick.RemoveListener(SignInGoogle);
		m_FacebookButton.onClick.RemoveListener(SignInFacebook);
	}

	void SignInApple() => SignIn(m_SocialProcessor.AttachAppleID);

	void SignInGoogle() => SignIn(m_SocialProcessor.AttachGoogleID);

	void SignInFacebook() => SignIn(m_SocialProcessor.AttachFacebookID);

	string GetLocalization(string _Key) => m_LocalizationProcessor.Get(_Key);

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
			UIErrorMenu errorMenu = m_MenuProcessor.GetMenu<UIErrorMenu>();
			if (errorMenu != null)
			{
				errorMenu.Setup(
					"sign_in_error",
					"main_menu",
					GetLocalization("SIGN_IN_ERROR_TITLE"),
					GetLocalization("COMMON_ERROR_MESSAGE")
				);
			}
			
			await m_MenuProcessor.Show(MenuType.ErrorMenu, true);
			
			await m_MenuProcessor.Hide(MenuType.LoginMenu);
			
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		}
	}
}