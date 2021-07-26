using UnityEngine;
using Zenject;

[RequireComponent(typeof(Animator))]
public abstract class UIIndicator : UIEntity
{
	public abstract UIHandle Handle { get; }

	protected Animator Animator
	{
		get
		{
			if (m_Animator == null)
				m_Animator = GetComponent<Animator>();
			return m_Animator;
		}
	}

	protected SignalBus SignalBus { get; private set; }

	Animator m_Animator;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		SignalBus = _SignalBus;
		
		Animator.keepAnimatorControllerStateOnDisable = true;
	}
}