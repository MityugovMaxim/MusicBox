using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.LoginMenu)]
public class UILoginMenu : UIMenu
{
	[SerializeField] UILoader m_Loader;

	SocialProcessor       m_SocialProcessor;
	LevelProcessor        m_LevelProcessor;
	ScoreProcessor        m_ScoreProcessor;
	StoreProcessor        m_StoreProcessor;
	ProfileProcessor      m_ProfileProcessor;
	MenuProcessor         m_MenuProcessor;
	UrlProcessor          m_UrlProcessor;
	NotificationProcessor m_NotificationProcessor;

	[Inject]
	public void Construct(
		SocialProcessor  _SocialProcessor,
		LevelProcessor   _LevelProcessor,
		ScoreProcessor   _ScoreProcessor,
		StoreProcessor   _StoreProcessor,
		ProfileProcessor _ProfileProcessor,
		MenuProcessor    _MenuProcessor,
		UrlProcessor     _UrlProcessor
	)
	{
		m_SocialProcessor  = _SocialProcessor;
		m_LevelProcessor   = _LevelProcessor;
		m_ScoreProcessor   = _ScoreProcessor;
		m_StoreProcessor   = _StoreProcessor;
		m_ProfileProcessor = _ProfileProcessor;
		m_MenuProcessor    = _MenuProcessor;
		m_UrlProcessor     = _UrlProcessor;
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
			m_ProfileProcessor.LoadProfile(),
			m_LevelProcessor.LoadLevels(),
			m_ScoreProcessor.LoadScores(),
			m_StoreProcessor.LoadProducts(),
			m_StoreProcessor.LoadPurchases(),
		};
		
		await Task.WhenAll(tasks);
		
		//await m_UrlProcessor.ProcessURL(m_NotificationProcessor.LaunchURL);
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.LoginMenu);
	}
}
