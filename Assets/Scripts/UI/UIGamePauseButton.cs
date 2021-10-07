using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIGamePauseButton : UIEntity
{
	static readonly int m_ShowParameterID    = Animator.StringToHash("Show");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	MenuProcessor  m_MenuProcessor;
	LevelProcessor m_LevelProcessor;

	bool     m_Paused;
	Animator m_Animator;

	[Inject]
	public void Construct(MenuProcessor _MenuProcessor, LevelProcessor _LevelProcessor)
	{
		m_MenuProcessor  = _MenuProcessor;
		m_LevelProcessor = _LevelProcessor;
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

	public async void Resume()
	{
		if (!m_Paused)
			return;
		
		m_Paused = false;
		
		m_Animator.SetBool(m_ShowParameterID, false);
		
		await m_MenuProcessor.Hide(MenuType.PauseMenu);
		
		m_LevelProcessor.Play();
	}

	public void Pause()
	{
		if (m_Paused)
			return;
		
		m_Paused = true;
		
		m_Animator.SetBool(m_ShowParameterID, true);
		
		m_MenuProcessor.Show(MenuType.PauseMenu);
		
		m_LevelProcessor.Pause();
	}

	public void Toggle()
	{
		if (m_Paused)
			Resume();
		else
			Pause();
	}
}
