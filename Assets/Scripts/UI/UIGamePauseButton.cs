using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIGamePauseButton : UIEntity
{
	static readonly int m_ShowParameterID    = Animator.StringToHash("Show");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	MenuProcessor m_MenuProcessor;

	bool     m_Paused;
	Animator m_Animator;

	[Inject]
	public void Construct(MenuProcessor _MenuProcessor)
	{
		m_MenuProcessor = _MenuProcessor;
	}

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	public void Restore()
	{
		m_Paused = false;
		m_Animator.SetBool(m_ShowParameterID, false);
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	public void Resume()
	{
		if (!m_Paused)
			return;
		
		m_Paused = false;
		
		UIPauseMenu pauseMenu = m_MenuProcessor.GetMenu<UIPauseMenu>(MenuType.PauseMenu);
		
		if (pauseMenu != null)
			pauseMenu.Resume();
		
		m_Animator.SetBool(m_ShowParameterID, false);
	}

	public void Pause()
	{
		if (m_Paused)
			return;
		
		m_Paused = true;
		
		UIPauseMenu pauseMenu = m_MenuProcessor.GetMenu<UIPauseMenu>(MenuType.PauseMenu);
		
		if (pauseMenu != null)
			pauseMenu.Pause();
		
		m_Animator.SetBool(m_ShowParameterID, true);
	}

	public void Toggle()
	{
		if (m_Paused)
			Resume();
		else
			Pause();
	}
}
