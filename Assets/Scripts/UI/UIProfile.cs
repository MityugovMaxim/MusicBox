using TMPro;
using UnityEngine;
using Zenject;

public class UIProfile : UIEntity
{
	[SerializeField, Range(0, 1)] float         m_Phase;
	[SerializeField]              UIRemoteImage m_Avatar;
	[SerializeField]              TMP_Text      m_Username;
	[SerializeField]              TMP_Text      m_Level;
	[SerializeField]              RectTransform m_Progress;

	SignalBus         m_SignalBus;
	LanguageProcessor m_LanguageProcessor;
	ProfileProcessor  m_ProfileProcessor;
	SocialProcessor   m_SocialProcessor;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessPhase();
	}
	#endif

	[Inject]
	public void Construct(
		SignalBus         _SignalBus,
		LanguageProcessor _LanguageProcessor,
		ProfileProcessor  _ProfileProcessor,
		SocialProcessor   _SocialProcessor
	)
	{
		m_SignalBus         = _SignalBus;
		m_LanguageProcessor = _LanguageProcessor;
		m_ProfileProcessor  = _ProfileProcessor;
		m_SocialProcessor   = _SocialProcessor;
		
		Refresh();
		
		m_SignalBus.Subscribe<LoginSignal>(RegisterLogin);
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(RegisterProfileDataUpdate);
	}

	void ProcessPhase()
	{
		Vector2 anchor = m_Progress.anchorMax;
		anchor.x             = m_Phase;
		m_Progress.anchorMax = anchor;
	}

	void RegisterLogin()
	{
		Refresh();
	}

	void RegisterProfileDataUpdate()
	{
		Refresh();
	}

	void Refresh()
	{
		if (m_Avatar != null)
			m_Avatar.Load(m_SocialProcessor.Photo);
		
		if (m_Username != null)
			m_Username.text = m_SocialProcessor.Guest ? m_LanguageProcessor.Get("PROFILE_GUEST") : m_SocialProcessor.Name;
		
		if (m_Level != null)
			m_Level.text = m_ProfileProcessor.Level.ToString();
	}
}