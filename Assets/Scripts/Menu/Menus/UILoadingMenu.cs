using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.LoadingMenu)]
public class UILoadingMenu : UIMenu
{
	const string TUTORIAL_KEY = "TUTORIAL";

	static bool Tutorial
	{
		get => PlayerPrefs.GetInt(TUTORIAL_KEY, 0) > 0;
		set => PlayerPrefs.SetInt(TUTORIAL_KEY, value ? 1 : 0);
	}

	[SerializeField] UISongImage m_Image;

	[Inject] TutorialController m_TutorialController;
	[Inject] SongController     m_SongController;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] AudioManager       m_AudioManager;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.Setup(m_SongID);
	}

	public async void Load()
	{
		await m_MenuProcessor.Hide(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.SongMenu, true);
		
		await ProcessAudioOutput();
		
		if (Tutorial)
			await LoadSong();
		else
			await LoadTutorial();
	}

	async Task LoadTutorial()
	{
		Tutorial = true;
		
		Task<bool> load = m_TutorialController.Load(m_SongID);
		
		await Task.WhenAll(
			load,
			Task.Delay(2000)
		);
		
		bool success = load.Result;
		
		if (success)
		{
			await Task.Delay(500);
			
			await m_MenuProcessor.Hide(MenuType.LoadingMenu);
			
			m_TutorialController.Start();
		}
		else
		{
			UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
			
			songMenu.Setup(m_SongID);
			
			await m_MenuProcessor.Show(MenuType.MainMenu, true);
			await m_MenuProcessor.Show(MenuType.SongMenu, true);
			
			await m_MenuProcessor.ErrorLocalizedAsync(
				"tutorial_load",
				"TUTORIAL_LOAD_ERROR_TITLE",
				"TUTORIAL_LOAD_ERROR_MESSAGE"
			);
			
			await m_MenuProcessor.Hide(MenuType.LoadingMenu);
		}
	}

	async Task LoadSong()
	{
		Task<bool> load = m_SongController.Load(m_SongID);
		
		await Task.WhenAll(
			load,
			Task.Delay(2000)
		);
		
		bool success = load.Result;
		
		if (success)
		{
			await Task.Delay(500);
			
			await m_MenuProcessor.Hide(MenuType.LoadingMenu);
			
			m_SongController.Start();
		}
		else
		{
			UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
			
			songMenu.Setup(m_SongID);
			
			await m_MenuProcessor.Show(MenuType.MainMenu, true);
			await m_MenuProcessor.Show(MenuType.SongMenu, true);
			
			await m_MenuProcessor.ErrorLocalizedAsync(
				"song_load",
				"SONG_LOAD_ERROR_TITLE",
				"SONG_LOAD_ERROR_MESSAGE"
			);
			
			await m_MenuProcessor.Hide(MenuType.LoadingMenu);
		}
	}

	async Task ProcessAudioOutput()
	{
		if (m_AudioManager.HasSettings())
			return;
		
		await m_MenuProcessor.Show(MenuType.SetupMenu);
		
		UISetupMenu setupMenu = m_MenuProcessor.GetMenu<UISetupMenu>();
		
		await setupMenu.Process();
		
		await m_MenuProcessor.Hide(MenuType.SetupMenu);
		
		await Task.Delay(500);
	}
}