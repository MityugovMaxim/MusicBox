using System;
using TMPro;
using UnityEngine;
using Zenject;

public class UIProfile : UIEntity
{
	public string Username
	{
		get => m_Username;
		set
		{
			if (m_Username == value)
				return;
			
			m_Username = value;
			
			m_UsernameLabel.text = m_Username;
		}
	}

	public Uri Avatar
	{
		get => m_Avatar;
		set
		{
			if (m_Avatar == value)
				return;
			
			m_Avatar = value;
			
			m_AvatarImage.Load(m_Avatar);
		}
	}

	public float Progress
	{
		get => m_Progress;
		set
		{
			if (Mathf.Approximately(m_Progress, value))
				return;
			
			m_Progress = value;
			
			Vector2 anchor = m_ProgressBar.anchorMax;
			anchor.x                = m_Progress;
			m_ProgressBar.anchorMax = anchor;
		}
	}

	public int Level
	{
		get => m_Level;
		set
		{
			if (m_Level == value)
				return;
			
			m_Level = value;
			
			m_LevelLabel.text = m_Level.ToString();
		}
	}

	public long Coins
	{
		get => m_Coins;
		set
		{
			if (m_Coins == value)
				return;
			
			m_Coins = value;
			
			m_CoinsLabel.text = m_Coins.ToString();
		}
	}

	[SerializeField] UIRemoteImage m_AvatarImage;
	[SerializeField] TMP_Text      m_UsernameLabel;
	[SerializeField] TMP_Text      m_LevelLabel;
	[SerializeField] TMP_Text      m_CoinsLabel;
	[SerializeField] RectTransform m_ProgressBar;

	Uri    m_Avatar;
	string m_Username;
	float  m_Progress;
	int    m_Level;
	long   m_Coins;
	bool   m_Locked;

	SignalBus         m_SignalBus;
	LanguageProcessor m_LanguageProcessor;
	ProfileProcessor  m_ProfileProcessor;
	SocialProcessor   m_SocialProcessor;

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

	public void Lock()
	{
		m_Locked = true;
	}

	public void Unlock()
	{
		m_Locked = false;
		
		Refresh();
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
		if (m_Locked)
			return;
		
		Username = m_SocialProcessor.Guest ? m_LanguageProcessor.Get("PROFILE_GUEST") : m_SocialProcessor.Name;
		Avatar   = m_SocialProcessor.Photo;
		Level    = m_ProfileProcessor.GetLevel();
		Progress = m_ProfileProcessor.GetProgress();
	}
}