using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.LoginMenu)]
public class UILoginMenu : UIMenu
{
	const string LAUNCH_COUNT_KEY = "LAUNCH_COUNT";

	int LaunchCount
	{
		get => PlayerPrefs.GetInt(LAUNCH_COUNT_KEY, 0);
		set => PlayerPrefs.SetInt(LAUNCH_COUNT_KEY, value);
	}

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
	[Inject] UrlProcessor         m_UrlProcessor;
	[Inject] SongsManager         m_SongsManager;

	public async Task Login()
	{
		while (true)
		{
			bool login = await m_SocialProcessor.Login();
			
			if (login)
				break;
			
			await Task.Delay(250);
		}
		
		await Task.WhenAll(
			m_ConfigProcessor.Load(),
			m_ApplicationProcessor.Load()
		);
		
		await m_LanguageProcessor.Load();
		
		await Task.WhenAny(
			m_AmbientProcessor.Load(),
			Task.Delay(150)
		);
		
		SongLibraryRequest request = new SongLibraryRequest();
		
		await request.SendAsync();
		
		await Task.WhenAll(
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
		
		await Task.WhenAll(
			m_StoreProcessor.Load(),
			m_AdsProcessor.Load()
		);
		
		LaunchCount++;
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		await m_MenuProcessor.Show(MenuType.BannerMenu, true);
		
		UIBannerMenu bannerMenu = m_MenuProcessor.GetMenu<UIBannerMenu>();
		if (bannerMenu != null)
			await bannerMenu.Process();
		
		await m_MenuProcessor.Hide(MenuType.BannerMenu, true);
		
		m_MenuProcessor.RemoveMenu(MenuType.BannerMenu);
		
		await m_MessageProcessor.LoadMessages(GetURLScheme());
		
		await m_MenuProcessor.Hide(MenuType.LoginMenu);
	}

	string GetURLScheme()
	{
		if (!string.IsNullOrEmpty(Application.absoluteURL) || LaunchCount > 1)
			return Application.absoluteURL;
		
		string songID = m_SongsManager
			.GetLibrarySongIDs()
			.FirstOrDefault();
		
		return m_UrlProcessor.GetSongURL(songID);
	}
}
