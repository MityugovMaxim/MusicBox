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

	ProfileProcessor   m_ProfileProcessor;
	ProgressProcessor  m_ProgressProcessor;
	SocialProcessor    m_SocialProcessor;
	MenuProcessor      m_MenuProcessor;
	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;

	[Inject]
	public void Construct(
		ProfileProcessor   _ProfileProcessor,
		ProgressProcessor  _ProgressProcessor,
		SocialProcessor    _SocialProcessor,
		MenuProcessor      _MenuProcessor,
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_ProfileProcessor   = _ProfileProcessor;
		m_ProgressProcessor  = _ProgressProcessor;
		m_SocialProcessor    = _SocialProcessor;
		m_MenuProcessor      = _MenuProcessor;
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
	}

	public void Setup()
	{
		ProcessUsername();
		
		ProcessDiscs();
		
		m_Avatar.Load(m_SocialProcessor.Photo);
		m_Coins.text  = $"{m_ProfileProcessor.Coins}<sprite name=coins_icon>";
		m_Level.Level = m_ProfileProcessor.Level;
		
		Vector2 size = m_Progress.sizeDelta;
		size.x = Mathf.Lerp(m_MinProgress, m_MaxProgress, m_ProfileProcessor.GetProgress());
		m_Progress.sizeDelta = size;
	}

	public void Open()
	{
		m_StatisticProcessor.LogMainMenuProfileClick();
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null && mainMenu.Shown)
			mainMenu.Select(MainMenuPageType.Profile);
	}

	void ProcessDiscs()
	{
		int minLevel     = m_ProgressProcessor.GetMinLevel();
		int maxLevel     = m_ProgressProcessor.GetMaxLevel();
		int level        = Mathf.Clamp(m_ProfileProcessor.Level, minLevel, maxLevel);
		int currentDiscs = m_ProfileProcessor.Discs;
		int targetDiscs  = m_ProgressProcessor.GetMaxLimit(level);
		
		m_Discs.text = level < m_ProgressProcessor.GetMaxLevel()
			? $"{currentDiscs}/{targetDiscs}"
			: currentDiscs.ToString();
	}

	void ProcessUsername()
	{
		m_Username.text = m_SocialProcessor.GetUsername();
	}
}