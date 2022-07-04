using System;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

[Menu(MenuType.LoginMenu)]
public class UILoginMenu : UIMenu
{
	[Inject] SocialProcessor      m_SocialProcessor;
	[Inject] ConfigProcessor      m_ConfigProcessor;
	[Inject] ApplicationProcessor m_ApplicationProcessor;
	[Inject] AdsProcessor         m_AdsProcessor;
	[Inject] SongsProcessor       m_SongsProcessor;
	[Inject] ScoresProcessor      m_ScoresProcessor;
	[Inject] NewsProcessor        m_NewsProcessor;
	[Inject] OffersProcessor      m_OffersProcessor;
	[Inject] ProductsProcessor    m_ProductsProcessor;
	[Inject] RevivesProcessor     m_RevivesProcessor;
	[Inject] StoreProcessor       m_StoreProcessor;
	[Inject] ProgressProcessor    m_ProgressProcessor;
	[Inject] MessageProcessor     m_MessageProcessor;
	[Inject] ProfileProcessor     m_ProfileProcessor;
	[Inject] MenuProcessor        m_MenuProcessor;
	[Inject] LanguageProcessor    m_LanguageProcessor;
	[Inject] AmbientProcessor     m_AmbientProcessor;
	[Inject] BannersProcessor     m_BannersProcessor;
	[Inject] StatisticProcessor   m_StatisticProcessor;
	[Inject] UrlProcessor         m_UrlProcessor;
	[Inject] SongsManager         m_SongsManager;

	public async Task Login()
	{
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.Launch);
		
		while (true)
		{
			bool login = await m_SocialProcessor.Login();
			
			if (login)
				break;
			
			await Task.Delay(250);
		}
		
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.Login);
		
		m_StatisticProcessor.LogLogin(m_SocialProcessor.UserID, m_SocialProcessor.Name);
		
		Log.Info(this, "Login complete. User ID: {0}.", m_SocialProcessor.UserID);
		
		await LoadApplication();
		
		Log.Info(
			this,
			"Load application complete. Client Version: {0}. Server Version: {1}",
			m_ApplicationProcessor.ClientVersion,
			m_ApplicationProcessor.ServerVersion
		);
		
		await LoadLocalization();
		
		Log.Info(this, "Load localization complete. Language: {0}", m_LanguageProcessor.Language);
		
		await LoadAmbient();
		
		Log.Info(this, "Load ambient complete.");
		
		await LoadLibrary();
		
		Log.Info(this, "Load library complete.");
		
		await LoadData();
		
		Log.Info(this, "Load data complete.");
		
		await LoadMonetization();
		
		Log.Info(this, "Load monetization complete.");
		
		await LoadViews();
	}

	Task LoadApplication()
	{
		return Task.WhenAll(
			m_ConfigProcessor.Load(),
			m_ApplicationProcessor.Load()
		);
	}

	Task LoadLocalization()
	{
		return m_LanguageProcessor.Load();
	}

	Task LoadAmbient()
	{
		return Task.WhenAny(
			m_AmbientProcessor.Load(),
			Task.Delay(150)
		);
	}

	Task LoadLibrary()
	{
		SongLibraryRequest request = new SongLibraryRequest();
		
		return request.SendAsync();
	}

	Task LoadData()
	{
		return Task.WhenAll(
			m_ProductsProcessor.Load(),
			m_OffersProcessor.Load(),
			m_NewsProcessor.Load(),
			m_ProgressProcessor.Load(),
			m_SongsProcessor.Load(),
			m_ScoresProcessor.Load(),
			m_RevivesProcessor.Load(),
			m_ProfileProcessor.Load(),
			m_BannersProcessor.Load()
		);
	}

	Task LoadMonetization()
	{
		return Task.WhenAll(
			m_StoreProcessor.Load(),
			m_AdsProcessor.Load()
		);
	}

	async Task LoadViews()
	{
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		await m_MenuProcessor.Show(MenuType.BannerMenu, true);
		
		UIBannerMenu bannerMenu = m_MenuProcessor.GetMenu<UIBannerMenu>();
		if (bannerMenu != null)
			await bannerMenu.Process();
		
		await m_MenuProcessor.Hide(MenuType.BannerMenu, true);
		
		m_MenuProcessor.RemoveMenu(MenuType.BannerMenu);
		
		await m_MessageProcessor.LoadMessages(GetURLScheme());
		
		await m_MenuProcessor.Hide(MenuType.LoginMenu);
		
		m_MessageProcessor.Schedule(
			"Audio Box",
			"Which song you will play for today?",
			"audiobox://play",
			TimeSpan.FromHours(24)
		);
	}

	string GetURLScheme()
	{
		if (!string.IsNullOrEmpty(Application.absoluteURL) || m_ProfileProcessor.Discs > 0)
			return Application.absoluteURL;
		
		string songID = m_SongsManager
			.GetLibrarySongIDs()
			.FirstOrDefault();
		
		return m_UrlProcessor.GetSongURL(songID);
	}
}
