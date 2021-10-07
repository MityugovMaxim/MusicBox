using System.Collections;
using UnityEngine;
using Zenject;

[Menu(MenuType.LoadingMenu)]
public class UILoadingMenu : UIMenu
{
	[SerializeField] UILoader m_Loader;

	LevelProcessor m_LevelProcessor;
	MenuProcessor  m_MenuProcessor;
	MusicProcessor m_MusicProcessor;

	string m_LevelID;

	[Inject]
	public void Construct(
		LevelProcessor _LevelProcessor,
		MenuProcessor  _MenuProcessor,
		MusicProcessor _MusicProcessor
	)
	{
		m_LevelProcessor = _LevelProcessor;
		m_MenuProcessor  = _MenuProcessor;
		m_MusicProcessor = _MusicProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
	}

	protected override void OnShowStarted()
	{
		if (m_Loader != null)
			m_Loader.Restore();
	}

	protected override void OnShowFinished()
	{
		if (m_Loader != null)
			m_Loader.Play();
		
		UIGameMenu gameMenu = m_MenuProcessor.GetMenu<UIGameMenu>();
		if (gameMenu != null)
			gameMenu.Setup(m_LevelID);
		
		UIPauseMenu pauseMenu = m_MenuProcessor.GetMenu<UIPauseMenu>();
		if (pauseMenu != null)
			pauseMenu.Setup(m_LevelID);
		
		m_MenuProcessor.Show(MenuType.GameMenu, true);
		m_MenuProcessor.Hide(MenuType.PauseMenu, true);
		
		m_LevelProcessor.Create(m_LevelID);
	}

	protected override void OnHideFinished()
	{
		m_MusicProcessor.StopMusic();
		m_MusicProcessor.StopAmbient();
		
		m_LevelProcessor.Play();
	}

	protected override IEnumerator HideAnimation(CanvasGroup _CanvasGroup, float _Duration)
	{
		yield return Resources.UnloadUnusedAssets();
		
		System.GC.Collect();
		
		yield return new WaitForSeconds(1.0f);
		
		yield return base.HideAnimation(_CanvasGroup, _Duration);
	}
}