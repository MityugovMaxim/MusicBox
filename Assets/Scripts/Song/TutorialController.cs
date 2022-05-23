using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class TutorialController
{
	const string TUTORIAL_PATH  = "tutorial";
	const float  TUTORIAL_SPEED = 800;

	[Inject] UISongContainer        m_SongContainer;
	[Inject] ConfigProcessor        m_ConfigProcessor;
	[Inject] MenuProcessor          m_MenuProcessor;
	[Inject] ScoreManager           m_ScoreManager;
	[Inject] HealthManager          m_HealthManager;
	[Inject] TutorialPlayer.Factory m_TutorialFactory;

	string         m_SongID;
	TutorialPlayer m_Player;

	public async Task<bool> Load(string _SongID)
	{
		m_SongID = _SongID;
		
		if (m_Player != null)
		{
			m_Player.Stop();
			m_Player.Clear();
			GameObject.Destroy(m_Player.gameObject);
		}
		
		m_Player = null;
		
		await ResourceManager.UnloadAsync();
		
		TutorialPlayer player = await ResourceManager.LoadAsync<TutorialPlayer>(TUTORIAL_PATH);
		
		float ratio    = m_ConfigProcessor.SongRatio;
		float speed    = TUTORIAL_SPEED;
		float duration = m_SongContainer.Size / speed;
		
		m_Player = m_TutorialFactory.Create(player);
		m_Player.RectTransform.SetParent(m_SongContainer.RectTransform, false);
		m_Player.Setup(ratio, duration, Finish);
		
		m_ScoreManager.Setup(m_SongID);
		m_HealthManager.Setup(null);
		
		m_Player.Time = -m_Player.Duration;
		m_Player.Sample();
		
		await UnityTask.Yield();
		
		return true;
	}

	public void Start()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Play failed. player is null");
			return;
		}
		
		m_HealthManager.Restore();
		m_ScoreManager.Restore();
		
		m_Player.Time = -m_Player.Duration;
		m_Player.Process();
	}

	async void Finish()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Finish failed. Player is null.");
			return;
		}
		
		if (string.IsNullOrEmpty(m_SongID))
		{
			await m_MenuProcessor.Show(MenuType.MainMenu);
			await m_MenuProcessor.Hide(MenuType.GameMenu, true);
			await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
			await m_MenuProcessor.Hide(MenuType.ReviveMenu, true);
			
			GameObject.Destroy(m_Player.gameObject);
		}
		else
		{
			UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
			
			loadingMenu.Setup(m_SongID);
			
			await m_MenuProcessor.Show(MenuType.LoadingMenu);
			
			GameObject.Destroy(m_Player.gameObject);
			
			m_Player = null;
			
			loadingMenu.Load();
		}
	}
}