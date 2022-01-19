using TMPro;
using UnityEngine;
using Zenject;

public class UIProfile : UIEntity
{
	[SerializeField] UIRemoteGraphic m_Avatar;
	[SerializeField] TMP_Text        m_Username;
	[SerializeField] UILevel         m_Level;
	[SerializeField] TMP_Text        m_Coins;
	[SerializeField] RectTransform   m_Progress;
	[SerializeField] TMP_Text        m_Discs;
	[SerializeField] float           m_MinProgress;
	[SerializeField] float           m_MaxProgress;

	SignalBus         m_SignalBus;
	LanguageProcessor m_LanguageProcessor;
	ProfileProcessor  m_ProfileProcessor;
	ProgressProcessor m_ProgressProcessor;
	SocialProcessor   m_SocialProcessor;

	[Inject]
	public void Construct(
		SignalBus         _SignalBus,
		LanguageProcessor _LanguageProcessor,
		ProfileProcessor  _ProfileProcessor,
		ProgressProcessor _ProgressProcessor,
		SocialProcessor   _SocialProcessor
	)
	{
		m_SignalBus         = _SignalBus;
		m_LanguageProcessor = _LanguageProcessor;
		m_ProfileProcessor  = _ProfileProcessor;
		m_ProgressProcessor = _ProgressProcessor;
		m_SocialProcessor   = _SocialProcessor;
		
		Refresh();
		
		m_SignalBus.Subscribe<SocialDataUpdateSignal>(RegisterSocialDataUpdate);
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(RegisterProfileDataUpdate);
		m_SignalBus.Subscribe<ProgressDataUpdateSignal>(RegisterProgressDataUpdate);
	}

	void RegisterSocialDataUpdate()
	{
		Refresh();
	}

	void RegisterProfileDataUpdate()
	{
		Refresh();
	}

	void RegisterProgressDataUpdate()
	{
		Refresh();
	}

	void Refresh()
	{
		ProcessUsername();
		
		ProcessDiscs();
		
		m_Avatar.Load(@"https://interactive-examples.mdn.mozilla.net/media/cc0-images/grapefruit-slice-332-332.jpg");
		m_Coins.text  = $"{m_ProfileProcessor.Coins}<sprite tint=1 name=unit_font_coins>";
		m_Level.Level = m_ProfileProcessor.GetLevel();
		
		Vector2 size = m_Progress.sizeDelta;
		size.x = Mathf.Lerp(m_MinProgress, m_MaxProgress, m_ProfileProcessor.GetProgress());
		m_Progress.sizeDelta = size;
	}

	void ProcessDiscs()
	{
		int level        = m_ProfileProcessor.GetLevel();
		int currentDiscs = m_ProfileProcessor.Discs;
		int targetDiscs  = m_ProgressProcessor.GetMaxLimit(level);
		
		m_Discs.text = $"{currentDiscs}/{targetDiscs}";
	}

	void ProcessUsername()
	{
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