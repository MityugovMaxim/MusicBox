using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIGamePauseButton : UIEntity
{
	static readonly int m_ShowParameterID    = Animator.StringToHash("Show");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	string   m_LevelID;
	bool     m_Paused;
	Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
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
		
		// TODO: Fix
		// m_SongsController.Play();
	}

	public async void Pause()
	{
		if (m_Paused)
			return;
		
		m_Paused = true;
		
		m_Animator.SetBool(m_ShowParameterID, true);
		
		// TODO: Fix
		// m_SongsController.Pause();
		
		await m_MenuProcessor.Show(MenuType.PauseMenu);
	}

	public void Toggle()
	{
		if (m_Paused)
		{
			m_StatisticProcessor.LogGameMenuResumeClick(m_LevelID);
			Resume();
		}
		else
		{
			m_StatisticProcessor.LogGameMenuPauseClick(m_LevelID);
			Pause();
		}
	}
}
