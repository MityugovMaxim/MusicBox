using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIGamePauseButton : UIEntity
{
	static readonly int m_ShowParameterID    = Animator.StringToHash("Show");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	UIPauseMenu m_PauseMenu;

	bool     m_Paused;
	Animator m_Animator;

	[Inject]
	public void Construct(UIPauseMenu _PauseMenu)
	{
		m_PauseMenu = _PauseMenu;
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
		m_PauseMenu.Resume();
		m_Animator.SetBool(m_ShowParameterID, false);
	}

	public void Pause()
	{
		if (m_Paused)
			return;
		
		m_Paused = true;
		m_PauseMenu.Pause();
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
