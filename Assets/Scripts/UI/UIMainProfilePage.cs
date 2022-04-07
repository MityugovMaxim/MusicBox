using TMPro;
using UnityEngine;
using Zenject;

public class UIMainProfilePage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Profile;

	[SerializeField] UIProfileImage m_Image;
	[SerializeField] TMP_InputField m_Username;
	[SerializeField] UILevel        m_Level;
	[SerializeField] TMP_Text       m_Coins;
	[SerializeField] UILanguageItem m_Language;
	[SerializeField] GameObject     m_LoginControls;
	[SerializeField] GameObject     m_LogoutControls;

	[Inject] SignalBus          m_SignalBus;
	[Inject] LanguageProcessor  m_LanguageProcessor;
	[Inject] SocialProcessor    m_SocialProcessor;
	[Inject] ProfileProcessor   m_ProfileProcessor;
	[Inject] ProductsProcessor  m_ProductsProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] HapticProcessor    m_HapticProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_Username.onSubmit.AddListener(SetUsername);
	}

	public async void Logout()
	{
		m_StatisticProcessor.LogMainMenuProfilePageSignOutClick(m_SocialProcessor.Provider);
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await m_MenuProcessor.Show(MenuType.LoginMenu);
		
		await m_MenuProcessor.Hide(MenuType.MainMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		
		m_SocialProcessor.Logout();
		
		UILoginMenu loginMenu = m_MenuProcessor.GetMenu<UILoginMenu>();
		if (loginMenu != null)
			await loginMenu.Login();
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
		m_StatisticProcessor.LogMainMenuProfilePageCoinsClick();
		
		string productID = m_ProductsProcessor.GetCoinsProductID(m_ProfileProcessor.Coins);
		
		if (string.IsNullOrEmpty(productID))
			return;
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		
		if (productMenu == null)
			return;
		
		productMenu.Setup(productID);
		
		await m_MenuProcessor.Show(MenuType.ProductMenu);
	}

	protected override void OnShowStarted()
	{
		Refresh();
		
		m_SignalBus.Subscribe<SocialDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ScoreDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<LanguageDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<LanguageSelectSignal>(Refresh);
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<SocialDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<ScoreDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<LanguageDataUpdateSignal>(Refresh);
		m_SignalBus.Unsubscribe<LanguageSelectSignal>(Refresh);
	}

	void Refresh()
	{
		m_Image.Setup(m_SocialProcessor.Photo);
		
		m_Level.Level   = m_ProfileProcessor.Level;
		m_Coins.text    = $"{m_ProfileProcessor.Coins}<sprite tint=0 name=coins_icon>";
		m_Username.text = m_SocialProcessor.GetUsername();
		
		m_Language.Setup(m_LanguageProcessor.Language, SetLanguage);
		
		m_LoginControls.SetActive(m_SocialProcessor.Guest);
		m_LogoutControls.SetActive(!m_SocialProcessor.Guest);
	}

	async void SetUsername(string _Username)
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_SocialProcessor.SetUsername(_Username);
		
		m_HapticProcessor.Process(Haptic.Type.Success);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	async void SetLanguage(string _Language)
	{
		UILanguageMenu languageMenu = m_MenuProcessor.GetMenu<UILanguageMenu>();
		
		languageMenu.Setup(_Language);
		
		await m_MenuProcessor.Show(MenuType.LanguageMenu);
	}
}