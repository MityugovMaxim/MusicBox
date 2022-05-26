using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

[Menu(MenuType.LoadingMenu)]
public class UILoadingMenu : UIAnimationMenu
{
	const string TUTORIAL_KEY = "TUTORIAL";

	static bool Tutorial
	{
		get => PlayerPrefs.GetInt(TUTORIAL_KEY, 0) > 0;
		set => PlayerPrefs.SetInt(TUTORIAL_KEY, value ? 1 : 0);
	}

	[SerializeField] UISongImage m_Image;
	[SerializeField] UIGroup     m_ProgressGroup;
	[SerializeField] UIProgress  m_Progress;

	[SerializeField, Sound] string m_TransitionSound;
	[SerializeField, Sound] string m_DropSound;

	[Inject] TutorialController m_TutorialController;
	[Inject] SongController     m_SongController;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] AudioManager       m_AudioManager;
	[Inject] SoundProcessor     m_SoundProcessor;

	string m_SongID;

	CancellationTokenSource m_TokenSource;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.gameObject.SetActive(!string.IsNullOrEmpty(m_SongID));
		
		m_Image.Setup(m_SongID);
		
		m_ProgressGroup.Hide(true);
		
		m_Progress.Progress = 0;
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

	public void ResetTutorial()
	{
		Tutorial = false;
	}

	[UsedImplicitly]
	void PlayTransitionSound()
	{
		m_SoundProcessor.Play(m_TransitionSound);
	}

	[UsedImplicitly]
	void PlayDropSound()
	{
		m_SoundProcessor.Play(m_DropSound);
	}

	async Task LoadTutorial()
	{
		Tutorial = true;
		
		Task<bool> load = m_TutorialController.Load(m_SongID);
		
		await Task.WhenAll(
			load,
			Task.Delay(500)
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
		Task<bool> load = m_SongController.Load(m_SongID, ProcessProgress);
		
		StartProgress();
		
		await Task.WhenAll(
			load,
			Task.Delay(1500)
		);
		
		StopProgress();
		
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

	void ProcessProgress(float _Progress)
	{
		m_Progress.Progress = _Progress;
	}

	async void StartProgress()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			await UnityTask.Delay(15, token);
			
			await m_ProgressGroup.ShowAsync();
		}
		catch (TaskCanceledException) { }
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	void StopProgress()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_ProgressGroup.Hide();
	}
}