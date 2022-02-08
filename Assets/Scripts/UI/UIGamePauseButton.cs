using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIGamePauseButton : UIEntity
{
	static readonly int m_ShowParameterID    = Animator.StringToHash("Show");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	MenuProcessor      m_MenuProcessor;
	LevelProcessor     m_LevelProcessor;
	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;

	string   m_LevelID;
	bool     m_Paused;
	Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	[Inject]
	public void Construct(
		MenuProcessor      _MenuProcessor,
		LevelProcessor     _LevelProcessor,
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_MenuProcessor      = _MenuProcessor;
		m_LevelProcessor     = _LevelProcessor;
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
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
		m_HapticProcessor.Process(Haptic.Type.ImpactSoft);
		
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
