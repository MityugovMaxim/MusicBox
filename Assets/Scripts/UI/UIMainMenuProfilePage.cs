using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIMainMenuProfilePage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Profile;

	[SerializeField] UIUnitLabel    m_Coins;
	[SerializeField] UILanguageItem m_Language;
	[SerializeField] GameObject     m_LogoutControls;
	[SerializeField] Button         m_ReviewButton;
	[SerializeField] Button         m_PrivacyPolicyButton;
	[SerializeField] Button         m_TermsOfServiceButton;
	[SerializeField] string         m_PrivacyPolicyURL;
	[SerializeField] string         m_TermsOfServiceURL;

	[Inject] SocialProcessor  m_SocialProcessor;
	[Inject] ProductsManager  m_ProductsManager;
	[Inject] LanguagesManager m_LanguagesManager;
	[Inject] LevelParameter   m_LevelParameter;
	[Inject] CoinsParameter   m_CoinsParameter;
	[Inject] MenuProcessor    m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_ReviewButton.onClick.AddListener(Review);
		m_TermsOfServiceButton.onClick.AddListener(TermsOfService);
		m_PrivacyPolicyButton.onClick.AddListener(PrivacyPolicy);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_ReviewButton.onClick.RemoveListener(Review);
		m_TermsOfServiceButton.onClick.RemoveListener(TermsOfService);
		m_PrivacyPolicyButton.onClick.RemoveListener(PrivacyPolicy);
	}

	public async void Logout()
	{
		async void Process()
		{
			await m_MenuProcessor.Show(MenuType.BlockMenu, true);
			
			await m_MenuProcessor.Show(MenuType.LoginMenu);
			
			await m_MenuProcessor.Hide(MenuType.MainMenu, true);
			
			await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
			
			m_SocialProcessor.Logout();
			
			UILoginMenu loginMenu = m_MenuProcessor.GetMenu<UILoginMenu>();
			if (loginMenu != null)
				await loginMenu.Login();
		}
		
		await m_MenuProcessor.ConfirmLocalizedAsync(
			"confirm_sign_out",
			"SIGN_OUT_CONFIRM_TITLE",
			"SIGN_OUT_CONFIRM_MESSAGE",
			Process
		);
	}

	public async void Login()
	{
		await m_MenuProcessor.Show(MenuType.SocialMenu);
	}

	public async void Latency()
	{
		await m_MenuProcessor.Show(MenuType.LatencyMenu);
	}

	public async void OpenCoins()
	{
		string productID = m_ProductsManager.GetProductID(m_CoinsParameter.Value);
		
		if (string.IsNullOrEmpty(productID))
			return;
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		
		if (productMenu == null)
			return;
		
		productMenu.Setup(productID);
		
		await m_MenuProcessor.Show(MenuType.ProductMenu);
	}

	public async void Tutorial()
	{
		UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
		
		if (loadingMenu == null)
			return;
		
		loadingMenu.Setup(string.Empty);
		loadingMenu.ResetTutorial();
		
		await loadingMenu.ShowAsync();
		
		loadingMenu.Load();
	}

	protected override void OnShowStarted()
	{
		Refresh();
		
		m_SocialProcessor.OnLogin  += ProcessSocial;
		m_SocialProcessor.OnLogout += ProcessSocial;
	}

	protected override void OnHideStarted()
	{
		m_SocialProcessor.OnLogin  -= ProcessSocial;
		m_SocialProcessor.OnLogout -= ProcessSocial;
	}

	void Refresh()
	{
	}

	async void Review()
	{
		await m_MenuProcessor.Show(MenuType.ReviewMenu);
	}

	void ProcessSocial()
	{
		#if UNITY_EDITOR
		m_LogoutControls.SetActive(true);
		#else
		m_LogoutControls.SetActive(!m_SocialProcessor.Guest);
		#endif
	}

	void TermsOfService()
	{
		if (string.IsNullOrEmpty(m_TermsOfServiceURL))
			return;
		
		Application.OpenURL(m_TermsOfServiceURL);
	}

	void PrivacyPolicy()
	{
		if (string.IsNullOrEmpty(m_PrivacyPolicyURL))
			return;
		
		Application.OpenURL(m_PrivacyPolicyURL);
	}

	async void SetLanguage(string _Language)
	{
		UILanguageMenu languageMenu = m_MenuProcessor.GetMenu<UILanguageMenu>();
		
		languageMenu.Setup(_Language);
		
		await m_MenuProcessor.Show(MenuType.LanguageMenu);
	}
}
