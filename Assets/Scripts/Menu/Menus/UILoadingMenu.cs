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
		get => PlayerPrefs.GetInt(TUTORIAL_KEY, 0) == 0;
		set => PlayerPrefs.SetInt(TUTORIAL_KEY, value ? 0 : 1);
	}

	[SerializeField] UISongImage m_Image;
	[SerializeField] UIGroup     m_ProgressGroup;
	[SerializeField] UIProgress  m_Progress;

	[SerializeField, Sound] string m_LoadSound;
	[SerializeField, Sound] string m_PlaySound;
	[SerializeField, Sound] string m_DropSound;

	[Inject] TutorialController m_TutorialController;
	[Inject] SongController     m_SongController;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] SoundProcessor     m_SoundProcessor;
	[Inject] HapticProcessor    m_HapticProcessor;

	string m_SongID;

	CancellationTokenSource m_TokenSource;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Image.gameObject.SetActive(!string.IsNullOrEmpty(m_SongID));
		
		m_Image.SongID = m_SongID;
		
		m_ProgressGroup.Hide(true);
		
		m_Progress.Progress = 0;
	}

	public async void Load()
	{
		if (Tutorial)
			await LoadTutorial();
		else
			await LoadSong();
	}

	public void ResetTutorial()
	{
		Tutorial = true;
	}

	async Task LoadTutorial()
	{
		Tutorial = false;
		
		Task<bool> load = m_TutorialController.Load(m_SongID);
		
		await Task.WhenAll(
			load,
			Task.Delay(500)
		);
		
		bool success = load.Result;
		
		if (success)
		{
			await Task.Delay(500);
			
			UITutorialMenu tutorialMenu = m_MenuProcessor.GetMenu<UITutorialMenu>();
			if (tutorialMenu != null)
				tutorialMenu.Show(true);
			
			await m_MenuProcessor.Hide(MenuType.MainMenu, true);
			await m_MenuProcessor.Hide(MenuType.LoadingMenu);
			
			m_TutorialController.Start();
		}
		else
		{
			UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
			
			songMenu.Setup(m_SongID);
			
			await m_MenuProcessor.Show(MenuType.MainMenu, true);
			await m_MenuProcessor.Show(MenuType.SongMenu, true);
			
			await m_MenuProcessor.ErrorAsync("tutorial_load");
			
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
			
			UIGameMenu gameMenu = m_MenuProcessor.GetMenu<UIGameMenu>();
			if (gameMenu != null)
			{
				gameMenu.Setup(m_SongID);
				gameMenu.Show(true);
			}
			
			UIPauseMenu pauseMenu = m_MenuProcessor.GetMenu<UIPauseMenu>();
			if (pauseMenu != null)
			{
				pauseMenu.Setup(m_SongID);
				pauseMenu.Hide(true);
			}
			
			await m_MenuProcessor.Hide(MenuType.MainMenu, true);
			await m_MenuProcessor.Hide(MenuType.LoadingMenu);
			
			m_SongController.Start();
		}
		else
		{
			UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
			
			songMenu.Setup(m_SongID);
			
			await m_MenuProcessor.Show(MenuType.MainMenu, true);
			await m_MenuProcessor.Show(MenuType.SongMenu, true);
			
			await m_MenuProcessor.ErrorAsync("song_load");
			
			await m_MenuProcessor.Hide(MenuType.LoadingMenu);
		}
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

	[UsedImplicitly]
	void ProcessLoad()
	{
		if (!m_Image.gameObject.activeSelf)
			return;
		
		m_SoundProcessor.Play(m_LoadSound);
	}

	[UsedImplicitly]
	void ProcessPlay()
	{
		if (!m_Image.gameObject.activeSelf)
			return;
		
		m_SoundProcessor.Play(m_PlaySound);
	}

	[UsedImplicitly]
	void ProcessDrop()
	{
		if (!m_Image.gameObject.activeSelf)
			return;
		
		m_SoundProcessor.Play(m_DropSound);
		m_HapticProcessor.Process(Haptic.Type.ImpactSoft);
	}
}
