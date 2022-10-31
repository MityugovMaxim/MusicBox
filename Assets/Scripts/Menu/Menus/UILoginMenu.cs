using System;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

[Menu(MenuType.LoginMenu)]
public class UILoginMenu : UIMenu
{
	const int LOGIN_ATTEMPT_LIMIT = 2;

	[Inject] RolesProcessor       m_RolesProcessor;
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
	[Inject] DailyProcessor       m_DailyProcessor;
	[Inject] LinkProcessor        m_LinkProcessor;

	public async Task Login()
	{
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.Launch);
		
		int attempt = 0;
		
		while (true)
		{
			bool login = await m_SocialProcessor.Login();
			
			if (login)
				break;
			
			await Task.Delay(150);
			
			attempt++;
			
			if (attempt < LOGIN_ATTEMPT_LIMIT)
				continue;
			
			TaskCompletionSource<bool> retry = new TaskCompletionSource<bool>();
			
			await m_MenuProcessor.RetryLocalizedAsync(
				"login",
				"login_menu",
				"LOGIN_ERROR_TITLE",
				"LOGIN_ERROR_MESSAGE",
				() => retry.TrySetResult(true)
			);
			
			await retry.Task;
			
			await Task.Delay(250);
			
			attempt = 0;
		}
		
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.Login);
		
		m_StatisticProcessor.LogLogin(m_SocialProcessor.UserID, m_SocialProcessor.Name);
		
		Log.Info(this, "Login complete. User ID: {0}.", m_SocialProcessor.UserID);
		
		m_LinkProcessor.Load();
		
		try
		{
			await LoadAdmin();
		}
		catch (Exception)
		{
			// Ignored
		}
		
		await LoadApplication();
		
		Log.Info(
			this,
			"Load application complete. Client Version: {0}. Server Version: {1}",
			m_ApplicationProcessor.ClientVersion,
			m_ApplicationProcessor.ServerVersion
		);
		
		await LoadLocalization();
		
		Log.Info(this, "Load localization complete. Language: {0}", m_LanguageProcessor.Language);
		
		await LoadMessages();
		
		Log.Info(this, "Load messages complete.");
		
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

	Task LoadAdmin() => AdminMode.Enabled ? m_RolesProcessor.Load() : Task.CompletedTask;

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

	Task LoadMessages()
	{
		return m_MessageProcessor.Load();
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
			m_BannersProcessor.Load(),
			m_DailyProcessor.Load()
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
		await m_MenuProcessor.Show(MenuType.BannerMenu, true);
		
		UIBannerMenu bannerMenu = m_MenuProcessor.GetMenu<UIBannerMenu>();
		if (bannerMenu != null)
			await bannerMenu.Process();
		
		await m_MenuProcessor.Hide(MenuType.BannerMenu, true);
		
		m_MenuProcessor.RemoveMenu(MenuType.BannerMenu);
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.LoginMenu);
		
		m_MessageProcessor.Schedule(
			"launch",
			Application.productName,
			GetLocalization("COMMON_NOTIFICATION"),
			"audiobox://play",
			TimeSpan.FromHours(24)
		);
	}
}
