using System;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UITutorialMenu : UIMenu, IInitializable, IDisposable
{
	static readonly int m_PlayParameterID = Animator.StringToHash("Play");

	SignalBus     m_SignalBus;
	UILoadingMenu m_LoadingMenu;

	Animator       m_Animator;
	StateBehaviour m_PlayState;

	[Inject]
	public void Construct(SignalBus _SignalBus, UILoadingMenu _LoadingMenu)
	{
		m_SignalBus   = _SignalBus;
		m_LoadingMenu = _LoadingMenu;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
	}

	void RegisterLevelStart()
	{
		Hide(true);
	}

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_PlayState = StateBehaviour.GetBehaviour(m_Animator, "play");
		if (m_PlayState != null)
			m_PlayState.OnComplete += InvokePlayFinished;
	}

	protected override void OnShowFinished()
	{
		m_Animator.SetTrigger(m_PlayParameterID);
	}

	void InvokePlayFinished()
	{
		m_LoadingMenu.Show();
	}
}