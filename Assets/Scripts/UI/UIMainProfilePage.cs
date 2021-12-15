using TMPro;
using UnityEngine;
using Zenject;

public class UIMainProfilePage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Profile;

	[SerializeField] UIRemoteImage m_Avatar;
	[SerializeField] TMP_Text      m_NameLabel;
	[SerializeField] UICoinsLabel  m_CoinsLabel;
	[SerializeField] TMP_Text      m_BronzeDiscsLabel;
	[SerializeField] TMP_Text      m_SilverDiscsLabel;
	[SerializeField] TMP_Text      m_GoldDiscsLabel;
	[SerializeField] TMP_Text      m_PlatinumDiscsLabel;
	[SerializeField] GameObject    m_LoginControls;
	[SerializeField] GameObject    m_LogoutControls;

	SignalBus        m_SignalBus;
	SocialProcessor  m_SocialProcessor;
	ProfileProcessor m_ProfileProcessor;
	ScoreProcessor   m_ScoreProcessor;
	MenuProcessor    m_MenuProcessor;

	[Inject]
	public void Construct(
		SignalBus        _SignalBus,
		SocialProcessor  _SocialProcessor,
		ProfileProcessor _ProfileProcessor,
		ScoreProcessor   _ScoreProcessor,
		MenuProcessor    _MenuProcessor
	)
	{
		m_SignalBus        = _SignalBus;
		m_SocialProcessor  = _SocialProcessor;
		m_ProfileProcessor = _ProfileProcessor;
		m_ScoreProcessor   = _ScoreProcessor;
		m_MenuProcessor    = _MenuProcessor;
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

	protected override void OnShowStarted()
	{
		Refresh();
		
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
		m_SignalBus.Subscribe<ScoreDataUpdateSignal>(Refresh);
	}

	protected override void OnHideFinished()
	{
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
		m_CoinsLabel.Coins = m_ProfileProcessor.Coins;
		
		m_BronzeDiscsLabel.text   = m_ScoreProcessor.GetDiscsCount(ScoreRank.Bronze).ToString();
		m_SilverDiscsLabel.text   = m_ScoreProcessor.GetDiscsCount(ScoreRank.Silver).ToString();
		m_GoldDiscsLabel.text     = m_ScoreProcessor.GetDiscsCount(ScoreRank.Gold).ToString();
		m_PlatinumDiscsLabel.text = m_ScoreProcessor.GetDiscsCount(ScoreRank.Platinum).ToString();
		
		string name = m_SocialProcessor.Name;
		if (!string.IsNullOrEmpty(name))
		{
			m_NameLabel.text = name;
			return;
		}
		
		string email = m_SocialProcessor.Email;
		if (!string.IsNullOrEmpty(email))
		{
			m_NameLabel.text = email.Split('@')[0];
			return;
		}
		
		m_NameLabel.text = "Guest";
	}
}