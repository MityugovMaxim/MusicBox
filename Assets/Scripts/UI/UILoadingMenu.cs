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
		m_Loader.Restore();
		m_Loader.Play();
	}

	protected override async void OnShowFinished()
	{
		UIGameMenu gameMenu = m_MenuProcessor.GetMenu<UIGameMenu>();
		if (gameMenu != null)
			gameMenu.Setup(m_LevelID);
		
		UIPauseMenu pauseMenu = m_MenuProcessor.GetMenu<UIPauseMenu>();
		if (pauseMenu != null)
			pauseMenu.Setup(m_LevelID);
		
		UIReviveMenu reviveMenu = m_MenuProcessor.GetMenu<UIReviveMenu>();
		if (reviveMenu != null)
			reviveMenu.Setup(m_LevelID);
		
		await m_MenuProcessor.Show(MenuType.GameMenu, true);
		await m_MenuProcessor.Hide(MenuType.ReviveMenu, true);
		await m_MenuProcessor.Hide(MenuType.PauseMenu, true);
		
		await m_LevelProcessor.Load(m_LevelID);
		
		await m_MenuProcessor.Hide(MenuType.LoadingMenu);
	}

	protected override void OnHideFinished()
	{
		m_MusicProcessor.StopMusic();
		m_MusicProcessor.StopAmbient();
		
		// TODO: Introduce level controls then play level
		
		m_LevelProcessor.Play();
	}

	protected override IEnumerator ShowAnimation(CanvasGroup _CanvasGroup, float _Duration)
	{
		yield return base.ShowAnimation(_CanvasGroup, _Duration);
		
		yield return Resources.UnloadUnusedAssets();
		
		System.GC.Collect();
		
		yield return new WaitForSeconds(1.0f);
	}
}