using UnityEngine;
using Zenject;

public enum ResultMenuPageType
{
	Discs,
	Coins,
	Control
}

[Menu(MenuType.ResultMenu)]
public class UIResultMenu : UIMenu
{
	[SerializeField] UILevelBackground m_Background;

	[SerializeField] UIResultMenuPage[] m_Pages;

	MenuProcessor   m_MenuProcessor;
	LevelProcessor  m_LevelProcessor;
	AdsProcessor    m_AdsProcessor;
	HapticProcessor m_HapticProcessor;

	string m_LevelID;

	int m_RestartAdsCount;
	int m_LeaveAdsCount;
	int m_NextAdsCount;
	int m_RateUsCount;

	[Inject]
	public void Construct(
		MenuProcessor   _MenuProcessor,
		LevelProcessor  _LevelProcessor,
		SocialProcessor _SocialProcessor,
		AdsProcessor    _AdsProcessor,
		HapticProcessor _HapticProcessor
	)
	{
		m_MenuProcessor   = _MenuProcessor;
		m_LevelProcessor  = _LevelProcessor;
		m_AdsProcessor    = _AdsProcessor;
		m_HapticProcessor = _HapticProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		m_Background.Setup(m_LevelID, true);
		
		foreach (UIResultMenuPage page in m_Pages)
			page.Setup(m_LevelID);
	}

	public void Select(ResultMenuPageType _PageType)
	{
		foreach (UIResultMenuPage page in m_Pages)
		{
			if (page.Type == _PageType)
				page.Show();
			else
				page.Hide();
		}
	}

	protected override void OnShowFinished()
	{
		m_LevelProcessor.Pause();
		
		Select(ResultMenuPageType.Discs);
	}

	protected override void OnHideFinished()
	{
		foreach (UIResultMenuPage page in m_Pages)
			page.Hide(true);
	}
}