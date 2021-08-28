using System.Collections;
using UnityEngine;
using Zenject;

public class UILoadingMenu : UIMenu
{
	[SerializeField] UILoader m_Loader;

	LevelProcessor m_LevelProcessor;
	MenuProcessor  m_MenuProcessor;

	string m_LevelID;

	[Inject]
	public void Construct(
		LevelProcessor _LevelProcessor,
		MenuProcessor  _MenuProcessor
	)
	{
		m_LevelProcessor = _LevelProcessor;
		m_MenuProcessor  = _MenuProcessor;
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
		
		m_MenuProcessor.Hide(MenuType.NotificationMenu, true);
		
		UIGameMenu gameMenu = m_MenuProcessor.GetMenu<UIGameMenu>(MenuType.GameMenu);
		if (gameMenu != null)
			gameMenu.Setup(m_LevelID);
		
		m_MenuProcessor.Show(MenuType.GameMenu, true);
		
		if (m_LevelProcessor != null)
			m_LevelProcessor.Create(m_LevelID);
	}

	protected override void OnHideFinished()
	{
		m_LevelProcessor.Play();
	}

	protected override IEnumerator HideAnimation(CanvasGroup _CanvasGroup, float _Duration)
	{
		AsyncOperation operation = Resources.UnloadUnusedAssets();
		
		yield return new WaitUntil(() => operation.isDone);
		
		yield return new WaitForSeconds(1.0f);
		
		yield return base.HideAnimation(_CanvasGroup, _Duration);
	}
}