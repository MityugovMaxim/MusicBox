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

	SignalBus         m_SignalBus;
	LanguageProcessor m_LanguageProcessor;
	SocialProcessor   m_SocialProcessor;
	ProfileProcessor  m_ProfileProcessor;
	MenuProcessor     m_MenuProcessor;

	[Inject]
	public void Construct(
		SignalBus         _SignalBus,
		LanguageProcessor _LanguageProcessor,
		SocialProcessor   _SocialProcessor,
		ProfileProcessor  _ProfileProcessor,
		MenuProcessor     _MenuProcessor
	)
	{
		m_SignalBus         = _SignalBus;
		m_LanguageProcessor = _LanguageProcessor;
		m_SocialProcessor   = _SocialProcessor;
		m_ProfileProcessor  = _ProfileProcessor;
		m_MenuProcessor     = _MenuProcessor;
	}

	public async void SignInApple()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_SocialProcessor.AttachAppleID();
		
		Reload();
	}

	public async void SignInGoogle()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_SocialProcessor.AttachGoogleID();
		
		Reload();
	}

	public void Logout()
	{
		m_SocialProcessor.Logout();
		
		Reload();
	}

	public void ChangeUsername(string _Username)
	{
		if (string.IsNullOrEmpty(_Username))
			m_Username.text = m_SocialProcessor.Name;
		
		m_SocialProcessor.SetUsername(_Username);
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

	async void Reload()
	{
		await m_MenuProcessor.Show(MenuType.LoginMenu);
		await m_MenuProcessor.Hide(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.LevelMenu, true);
		await m_MenuProcessor.Hide(MenuType.ProductMenu, true);
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu, true);
	}

	void Refresh()
	{
		m_LoginControls.SetActive(m_SocialProcessor.Online && m_SocialProcessor.Guest);
		m_LogoutControls.SetActive(m_SocialProcessor.Online && !m_SocialProcessor.Guest);
		
		m_Avatar.Load(m_SocialProcessor.Photo);
		
		m_Level.Level = m_ProfileProcessor.GetLevel();
		m_Coins.text  = $"{m_ProfileProcessor.Coins}<sprite tint=1 name=unit_font_coins>";
		
		string username = m_SocialProcessor.Name;
		if (!string.IsNullOrEmpty(username))
		{
			m_Username.text = username;
			return;
		}
		
		string email = m_SocialProcessor.Email;
		if (!string.IsNullOrEmpty(email))
		{
			m_Username.text = email.Split('@')[0];
			return;
		}
		
		string device = SystemInfo.deviceName;
		if (!string.IsNullOrEmpty(device))
		{
			m_Username.text = device;
			return;
		}
		
		m_Username.text = m_SocialProcessor.Guest
			? m_LanguageProcessor.Get("PROFILE_GUEST")
			: SystemInfo.deviceModel;
	}
}