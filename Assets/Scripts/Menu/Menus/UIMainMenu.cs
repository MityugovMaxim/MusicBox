using System;
using UnityEngine;
using Zenject;

public enum MainMenuPageType
{
	News    = 0,
	Store   = 1,
	Songs   = 2,
	Chests  = 3,
	Seasons = 4,
}

[Menu(MenuType.MainMenu)]
public class UIMainMenu : UIMenu
{
	[SerializeField] UIMainMenuPage[]  m_Pages;
	[SerializeField] UIMainMenuControl m_Control;

	[Inject] LinkProcessor  m_LinkProcessor;
	[Inject] AmbientManager m_AmbientManager;
	[Inject] MenuProcessor  m_MenuProcessor;

	[NonSerialized] MainMenuPageType m_PageType = MainMenuPageType.Songs;

	async void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			UIDataMenu dataMenu = m_MenuProcessor.GetMenu<UIDataMenu>();
			
			AdminLanguagesData languages = new AdminLanguagesData();
			
			await languages.LoadAsync();
			
			AdminStoreData         store         = new AdminStoreData();
			AdminProductsData      products      = new AdminProductsData();
			AdminNewsData          news          = new AdminNewsData(languages.Languages);
			AdminOffersData        offers        = new AdminOffersData(languages.Languages);
			AdminSongsData         songs         = new AdminSongsData();
			AdminRevivesData       revives       = new AdminRevivesData();
			AdminDailyData         daily         = new AdminDailyData();
			AdminProgressData      progress      = new AdminProgressData();
			AdminDifficultyData    difficulty    = new AdminDifficultyData();
			AdminChestsData        chests        = new AdminChestsData();
			AdminVouchersData      vouchers      = new AdminVouchersData();
			AdminSeasonsData       seasons       = new AdminSeasonsData();
			AdminLocalizationsData localizations = new AdminLocalizationsData(languages.Languages);
			
			await store.LoadAsync();
			await products.LoadAsync();
			await news.LoadAsync();
			await offers.LoadAsync();
			await songs.LoadAsync();
			await revives.LoadAsync();
			await daily.LoadAsync();
			await progress.LoadAsync();
			await difficulty.LoadAsync();
			await chests.LoadAsync();
			await vouchers.LoadAsync();
			await seasons.LoadAsync();
			await localizations.LoadAsync();
			
			dataMenu.Setup(
				store,
				products,
				languages,
				news,
				offers,
				songs,
				revives,
				daily,
				progress,
				difficulty,
				chests,
				vouchers,
				seasons,
				localizations
			);
			
			dataMenu.Show();
		}
	}

	public void Select(MainMenuPageType _PageType, bool _Instant = false)
	{
		if (m_PageType == _PageType)
			return;
		
		foreach (UIMainMenuPage page in m_Pages)
		{
			if (page.Type == _PageType)
				page.Show(m_PageType, _Instant);
			else
				page.Hide(_PageType, _Instant);
		}
		
		m_PageType = _PageType;
		
		m_Control.Select(m_PageType, _Instant);
	}

	protected override async void OnShowStarted()
	{
		foreach (UIMainMenuPage page in m_Pages)
		{
			if (page.Type == m_PageType)
				page.Show(m_PageType, true);
			else
				page.Hide(m_PageType, true);
		}
		m_Control.Select(m_PageType, true);
		
		await m_LinkProcessor.Process(true);
		
		m_LinkProcessor.Subscribe(ProcessLink);
	}

	protected override void OnHideStarted()
	{
		m_LinkProcessor.Unsubscribe(ProcessLink);
	}

	protected override void OnHideFinished()
	{
		foreach (UIMainMenuPage page in m_Pages)
			page.Hide(m_PageType, true);
	}

	public override void OnFocusGain()
	{
		m_AmbientManager.Play();
	}

	public override void OnFocusLose()
	{
		m_AmbientManager.Pause();
	}

	async void ProcessLink() => await m_LinkProcessor.Process();
}
