using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

public class UIMainProfilePage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Profile;

	[SerializeField] UIRemoteGraphic m_Avatar;
	[SerializeField] TMP_InputField  m_Username;
	[SerializeField] UILevel         m_Level;
	[SerializeField] TMP_Text        m_Coins;
	[SerializeField] GameObject      m_LoginControls;
	[SerializeField] GameObject      m_LogoutControls;

	SignalBus          m_SignalBus;
	LanguageProcessor  m_LanguageProcessor;
	SocialProcessor    m_SocialProcessor;
	ProfileProcessor   m_ProfileProcessor;
	StoreProcessor     m_StoreProcessor;
	ProductProcessor   m_ProductProcessor;
	MenuProcessor      m_MenuProcessor;
	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;

	[Inject]
	public void Construct(
		SignalBus          _SignalBus,
		LanguageProcessor  _LanguageProcessor,
		SocialProcessor    _SocialProcessor,
		ProfileProcessor   _ProfileProcessor,
		StoreProcessor     _StoreProcessor,
		ProductProcessor   _ProductProcessor,
		MenuProcessor      _MenuProcessor,
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_SignalBus          = _SignalBus;
		m_LanguageProcessor  = _LanguageProcessor;
		m_SocialProcessor    = _SocialProcessor;
		m_ProfileProcessor   = _ProfileProcessor;
		m_StoreProcessor     = _StoreProcessor;
		m_ProductProcessor   = _ProductProcessor;
		m_MenuProcessor      = _MenuProcessor;
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
	}

	public void SignInApple()
	{
		m_StatisticProcessor.LogMainMenuProfilePageSignInClick("apple");
		
		SignIn(
			m_SocialProcessor.AttachAppleID,
			m_LanguageProcessor.Get("APPLE_SIGN_IN_ERROR_TITLE"),
			m_LanguageProcessor.Get("APPLE_SING_IN_ERROR_MESSAGE")
		);
	}

	public void SignInGoogle()
	{
		m_StatisticProcessor.LogMainMenuProfilePageSignInClick("google");
		
		SignIn(
			m_SocialProcessor.AttachGoogleID,
			m_LanguageProcessor.Get("GOOGLE_SIGN_IN_ERROR_TITLE"),
			m_LanguageProcessor.Get("GOOGLE_SIGN_IN_ERROR_MESSAGE")
		);
	}

	public void SignInFacebook()
	{
		m_StatisticProcessor.LogMainMenuProfilePageSignInClick("facebook");
		
		SignIn(
			m_SocialProcessor.AttachFacebookID,
			m_LanguageProcessor.Get("FACEBOOK_SIGN_IN_ERROR_TITLE"),
			m_LanguageProcessor.Get("FACEBOOK_SIGN_IN_ERROR_MESSAGE")
		);
	}

	public async void Logout()
	{
		m_StatisticProcessor.LogMainMenuProfilePageSignOutClick(m_SocialProcessor.Provider);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.LoginMenu);
		
		await m_MenuProcessor.Hide(MenuType.MainMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		
		m_SocialProcessor.Logout();
		
		UILoginMenu loginMenu = m_MenuProcessor.GetMenu<UILoginMenu>();
		if (loginMenu != null)
			await loginMenu.Login();
	}

	public async void RestorePurchases()
	{
		m_StatisticProcessor.LogMainMenuProfilePageRestorePurchasesClick();
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		
		await m_StoreProcessor.Restore();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void ChangeUsername(string _Username)
	{
		m_StatisticProcessor.LogMainMenuProfilePageUsernameClick();
		
		if (string.IsNullOrEmpty(_Username))
			m_Username.text = m_SocialProcessor.Name;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_SocialProcessor.SetUsername(_Username);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		
		m_HapticProcessor.Process(Haptic.Type.Success);
	}

	public void OpenCoins()
	{
		m_StatisticProcessor.LogMainMenuProfilePageCoinsClick();
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		string productID = m_ProductProcessor.GetCoinsProductID(m_ProfileProcessor.Coins);
		
		if (string.IsNullOrEmpty(productID))
			return;
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		
		if (productMenu == null)
			return;
		
		productMenu.Setup(productID);
		
		m_MenuProcessor.Show(MenuType.ProductMenu);
	}

	protected override void OnShowStarted()
	{
		Refresh();
		
		m_SignalBus.Subscribe<SocialDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ScoreDataUpdateSignal>(Refresh);
	}

	protected override void OnHideFinished()
	{
		m_SignalBus.Unsubscribe<SocialDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ScoreDataUpdateSignal>(Refresh);
	}

	async void SignIn(Func<Task<bool>> _SignInTask, string _Title, string _Message)
	{
		if (_SignInTask == null)
			return;
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.LoginMenu);
		
		bool success = await _SignInTask();
		
		if (success)
		{
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

	void Refresh()
	{
		m_LoginControls.SetActive(m_SocialProcessor.Guest);
		m_LogoutControls.SetActive(!m_SocialProcessor.Guest);
		
		m_Avatar.Load(m_SocialProcessor.Photo);
		
		m_Level.Level = m_ProfileProcessor.Level;
		m_Coins.text  = $"{m_ProfileProcessor.Coins}<sprite tint=0 name=coins_icon>";
		
		m_Username.text = m_SocialProcessor.GetUsername();
	}
}