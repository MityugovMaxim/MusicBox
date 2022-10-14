using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class TutorialController
{
	const string TUTORIAL_PATH  = "tutorial";
	const float  TUTORIAL_SPEED = 600;

	[Inject] ConfigProcessor        m_ConfigProcessor;
	[Inject] MenuProcessor          m_MenuProcessor;
	[Inject] ScoreManager           m_ScoreManager;
	[Inject] HealthManager          m_HealthManager;
	[Inject] TutorialPlayer.Factory m_TutorialFactory;
	[Inject] StatisticProcessor     m_StatisticProcessor;

	string         m_SongID;
	TutorialPlayer m_Player;

	public async Task<bool> Load(string _SongID)
	{
		m_SongID = _SongID;
		
		if (m_Player != null)
		{
			m_Player.Stop();
			m_Player.Clear();
			Object.Destroy(m_Player.gameObject);
		}
		
		m_Player = null;
		
		await ResourceManager.UnloadAsync();
		
		TutorialPlayer player = await ResourceManager.LoadAsync<TutorialPlayer>(TUTORIAL_PATH);
		if (ReferenceEquals(player, null))
		{
			Log.Error(this, "Load tutorial failed. Player with ID '{0}' is null.", TUTORIAL_PATH);
			return false;
		}
		
		float ratio = m_ConfigProcessor.SongRatio;
		float speed = TUTORIAL_SPEED;
		
		m_Player = m_TutorialFactory.Create(player);
		m_Player.Setup(ratio, speed, Finish);
		
		m_ScoreManager.Setup(m_SongID);
		m_HealthManager.Setup(null);
		
		UITutorialMenu tutorialMenu = m_MenuProcessor.GetMenu<UITutorialMenu>();
		if (tutorialMenu != null)
			m_Player.AddSampler(tutorialMenu.Sampler);
		
		await m_MenuProcessor.Show(MenuType.TutorialMenu, true);
		
		m_Player.Time = -m_Player.Duration;
		m_Player.Sample();
		
		await UnityTask.Yield();
		
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.TutorialStart);
		
		return true;
	}

	public void Start()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Play failed. Player is null");
			return;
		}
		
		m_HealthManager.Restore();
		m_ScoreManager.Restore();
		
		m_Player.Time = -m_Player.Duration;
		m_Player.Process();
	}

	public void Skip()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Skip failed. Player is null");
			return;
		}
		
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.TutorialSkip);
		
		m_Player.Stop();
		
		Finish();
	}

	async void Finish()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Finish failed. Player is null.");
			return;
		}
		
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.TutorialFinish);
		
		async Task DestroyPlayer()
		{
			await m_MenuProcessor.Hide(MenuType.TutorialMenu, true);
			await m_MenuProcessor.Hide(MenuType.GameMenu, true);
			await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
			await m_MenuProcessor.Hide(MenuType.ReviveMenu, true);
			
			Object.Destroy(m_Player.gameObject);
			
			m_MenuProcessor.RemoveMenu(MenuType.TutorialMenu);
			
			m_Player = null;
		}
		
		if (string.IsNullOrEmpty(m_SongID))
		{
			await Task.WhenAll(
				m_MenuProcessor.Show(MenuType.MainMenu),
				m_MenuProcessor.Show(MenuType.TransitionMenu)
			);
			
			await DestroyPlayer();
		}
		else
		{
			UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
			
			loadingMenu.Setup(m_SongID);
			
			await m_MenuProcessor.Show(MenuType.LoadingMenu);
			
			await DestroyPlayer();
			
			loadingMenu.Load();
		}
	}
}
