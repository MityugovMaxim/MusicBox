using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.LoginMenu)]
public class UILoginMenu : UIMenu
{
	[SerializeField] UILoader m_Loader;

	SocialProcessor      m_SocialProcessor;
	ApplicationProcessor m_ApplicationProcessor;
	AdsProcessor         m_AdsProcessor;
	LevelProcessor       m_LevelProcessor;
	ScoreProcessor       m_ScoreProcessor;
	NewsProcessor        m_NewsProcessor;
	OffersProcessor      m_OffersProcessor;
	ProductProcessor     m_ProductProcessor;
	StoreProcessor       m_StoreProcessor;
	ProgressProcessor    m_ProgressProcessor;
	MessageProcessor     m_MessageProcessor;
	ProfileProcessor     m_ProfileProcessor;
	MenuProcessor        m_MenuProcessor;
	LanguageProcessor    m_LanguageProcessor;

	[Inject]
	public void Construct(
		SocialProcessor      _SocialProcessor,
		ApplicationProcessor _ApplicationProcessor,
		AdsProcessor         _AdsProcessor,
		LevelProcessor       _LevelProcessor,
		ScoreProcessor       _ScoreProcessor,
		NewsProcessor        _NewsProcessor,
		OffersProcessor      _OffersProcessor,
		ProductProcessor     _ProductProcessor,
		StoreProcessor       _StoreProcessor,
		ProgressProcessor    _ProgressProcessor,
		MessageProcessor     _MessageProcessor,
		ProfileProcessor     _ProfileProcessor,
		LanguageProcessor    _LanguageProcessor,
		MenuProcessor        _MenuProcessor
	)
	{
		m_SocialProcessor      = _SocialProcessor;
		m_ApplicationProcessor = _ApplicationProcessor;
		m_AdsProcessor         = _AdsProcessor;
		m_LevelProcessor       = _LevelProcessor;
		m_ScoreProcessor       = _ScoreProcessor;
		m_NewsProcessor        = _NewsProcessor;
		m_OffersProcessor      = _OffersProcessor;
		m_ProductProcessor     = _ProductProcessor;
		m_StoreProcessor       = _StoreProcessor;
		m_ProgressProcessor    = _ProgressProcessor;
		m_MessageProcessor     = _MessageProcessor;
		m_ProfileProcessor     = _ProfileProcessor;
		m_LanguageProcessor    = _LanguageProcessor;
		m_MenuProcessor        = _MenuProcessor;
	}

	protected override void OnShowStarted()
	{
		m_Loader.Restore();
	}

	protected override void OnHideFinished()
	{
		m_Loader.Restore();
	}

	public async Task Login()
	{
		await m_SocialProcessor.Login();
		
		await m_ApplicationProcessor.LoadApplication();
		
		await Task.WhenAll(
			m_LanguageProcessor.LoadLocalization(),
			m_ProductProcessor.LoadProducts(),
			m_OffersProcessor.LoadOffers(),
			m_NewsProcessor.LoadNews(),
			m_ProgressProcessor.LoadProgress(),
			m_LevelProcessor.LoadLevels(),
			m_ScoreProcessor.LoadScores(),
			m_ProfileProcessor.LoadProfile()
		);
		
		await Task.WhenAll(
			m_StoreProcessor.LoadStore(),
			m_AdsProcessor.LoadAds()
		);
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		await m_MenuProcessor.Show(MenuType.BannerMenu, true);
		
		UIBannerMenu bannerMenu = m_MenuProcessor.GetMenu<UIBannerMenu>();
		if (bannerMenu != null)
			await bannerMenu.Process();
		
		await m_MenuProcessor.Hide(MenuType.BannerMenu);
		
		await m_MessageProcessor.ProcessPermission();
		
		await m_MessageProcessor.ProcessTopics();
		
		await m_MessageProcessor.ProcessLaunchURL();
		
		await m_MenuProcessor.Hide(MenuType.LoginMenu);
	}
}
