using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UITutorialMenu : UIMenu
{
	static readonly int m_PlayParameterID = Animator.StringToHash("Play");

	MenuProcessor  m_MenuProcessor;
	string         m_LevelID;
	Animator       m_Animator;
	StateBehaviour m_PlayState;

	[Inject]
	public void Construct(MenuProcessor _MenuProcessor)
	{
		m_MenuProcessor = _MenuProcessor;
	}

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_PlayState = StateBehaviour.GetBehaviour(m_Animator, "play");
		if (m_PlayState != null)
			m_PlayState.OnComplete += InvokePlayFinished;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
	}

	protected override void OnShowFinished()
	{
		m_Animator.SetTrigger(m_PlayParameterID);
	}

	void InvokePlayFinished()
	{
		UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>(MenuType.LoadingMenu);
		if (loadingMenu != null)
			loadingMenu.Setup(m_LevelID);
		
		m_MenuProcessor.Show(MenuType.LoadingMenu);
	}
}