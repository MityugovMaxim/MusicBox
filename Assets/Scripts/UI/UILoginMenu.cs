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
	PurchaseProcessor m_PurchaseProcessor;
	MenuProcessor     m_MenuProcessor;

	[Inject]
	public void Construct(
		SocialProcessor   _SocialProcessor,
		LevelProcessor    _LevelProcessor,
		ScoreProcessor    _ScoreProcessor,
		PurchaseProcessor _PurchaseProcessor,
		MenuProcessor     _MenuProcessor
	)
	{
		m_SocialProcessor   = _SocialProcessor;
		m_LevelProcessor    = _LevelProcessor;
		m_ScoreProcessor    = _ScoreProcessor;
		m_PurchaseProcessor = _PurchaseProcessor;
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
			m_LevelProcessor.LoadLevels(),
			m_ScoreProcessor.LoadScores(),
			m_PurchaseProcessor.LoadProducts(),
			m_PurchaseProcessor.LoadPurchases(),
		};
		
		await Task.WhenAll(tasks);
		
		m_PurchaseProcessor.LoadStore();
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.LoginMenu);
	}
}
