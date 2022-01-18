using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.LoginMenu)]
public class UILoginMenu : UIMenu
{
	[SerializeField] UILoader m_Loader;

	SocialProcessor   m_SocialProcessor;
	LevelProcessor    m_LevelProcessor;
	ScoreProcessor    m_ScoreProcessor;
	StoreProcessor    m_StoreProcessor;
	ProgressProcessor m_ProgressProcessor;
	MessageProcessor  m_MessageProcessor;
	ProfileProcessor  m_ProfileProcessor;
	MenuProcessor     m_MenuProcessor;
	LanguageProcessor m_LanguageProcessor;

	[Inject]
	public void Construct(
		SocialProcessor   _SocialProcessor,
		LevelProcessor    _LevelProcessor,
		ScoreProcessor    _ScoreProcessor,
		StoreProcessor    _StoreProcessor,
		ProgressProcessor _ProgressProcessor,
		MessageProcessor  _MessageProcessor,
		ProfileProcessor  _ProfileProcessor,
		LanguageProcessor _LanguageProcessor,
		MenuProcessor     _MenuProcessor
	)
	{
		m_SocialProcessor   = _SocialProcessor;
		m_LevelProcessor    = _LevelProcessor;
		m_ScoreProcessor    = _ScoreProcessor;
		m_StoreProcessor    = _StoreProcessor;
		m_ProgressProcessor = _ProgressProcessor;
		m_MessageProcessor  = _MessageProcessor;
		m_ProfileProcessor  = _ProfileProcessor;
		m_LanguageProcessor = _LanguageProcessor;
		m_MenuProcessor     = _MenuProcessor;
	}

	protected override void OnShowStarted()
	{
		m_Loader.Restore();
		m_Loader.Play();
	}

	protected override void OnShowFinished()
	{
		Login();
	}

	protected override void OnHideFinished()
	{
		m_Loader.Restore();
	}

	async void Login()
	{
		await m_SocialProcessor.Login();
		
		Task[] tasks =
		{
			m_LanguageProcessor.LoadLocalization(),
			m_ProgressProcessor.LoadProgress(),
			m_ProfileProcessor.LoadProfile(),
			m_LevelProcessor.LoadLevels(),
			m_ScoreProcessor.LoadScores(),
			m_StoreProcessor.LoadProducts(),
			m_StoreProcessor.LoadPurchases(),
		};
		
		await Task.WhenAll(tasks);
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		await m_MessageProcessor.ProcessPermission();
		
		await m_MessageProcessor.ProcessTopics();
		
		await m_MessageProcessor.ProcessLaunchURL();
		
		await m_MenuProcessor.Hide(MenuType.LoginMenu);
	}
}
