using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.LoadingMenu)]
public class UILoadingMenu : UIMenu
{
	[SerializeField] UISongImage m_Image;

	[Inject] SongController m_SongController;
	[Inject] MenuProcessor  m_MenuProcessor;
	[Inject] AudioManager   m_AudioManager;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.Setup(m_SongID);
	}

	protected override async void OnShowFinished()
	{
		await m_MenuProcessor.Hide(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.SongMenu, true);
		
		await ProcessAudioOutput();
		
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
				"song_load_error",
				"SONG_LOAD_ERROR_TITLE",
				"SONG_LOAD_ERROR_MESSAGE"
			);
			
			await m_MenuProcessor.Hide(MenuType.LoadingMenu);
		}
	}

	Task ProcessAudioOutput()
	{
		if (m_AudioManager.HasSettings())
			return Task.FromResult(true);
		
		UISetupMenu setupMenu = m_MenuProcessor.GetMenu<UISetupMenu>();
		
		return m_MenuProcessor.Show(MenuType.SetupMenu)
			.ContinueWith(_Task => UnityTask.While(() => setupMenu.Shown))
			.ContinueWith(_Task => Task.Delay(1000));
	}
}