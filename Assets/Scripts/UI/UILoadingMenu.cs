using System.Collections;
using UnityEngine;
using Zenject;

[Menu(MenuType.LoadingMenu)]
public class UILoadingMenu : UIMenu
{
	[SerializeField] UILoader m_Loader;

	LevelController  m_LevelController;
	MenuProcessor    m_MenuProcessor;
	AmbientProcessor m_AmbientProcessor;
	MusicProcessor   m_MusicProcessor;

	string m_LevelID;

	[Inject]
	public void Construct(
		LevelController  _LevelController,
		MenuProcessor    _MenuProcessor,
		AmbientProcessor _AmbientProcessor,
		MusicProcessor   _MusicProcessor
	)
	{
		m_LevelController  = _LevelController;
		m_MenuProcessor    = _MenuProcessor;
		m_AmbientProcessor = _AmbientProcessor;
		m_MusicProcessor   = _MusicProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
	}

	protected override void OnShowStarted()
	{
		m_Loader.Restore();
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
		
		await m_LevelController.Load(m_LevelID);
		
		await m_MenuProcessor.Hide(MenuType.LoadingMenu);
	}

	protected override void OnHideFinished()
	{
		m_AmbientProcessor.Pause();
		m_MusicProcessor.StopPreview();
		
		// TODO: Introduce song ui elements before playing sequencer
		
		m_LevelController.Play();
	}

	protected override IEnumerator ShowAnimation(CanvasGroup _CanvasGroup, float _Duration)
	{
		yield return base.ShowAnimation(_CanvasGroup, _Duration);
		
		yield return Resources.UnloadUnusedAssets();
		
		System.GC.Collect();
		
		yield return new WaitForSeconds(1.0f);
	}
}