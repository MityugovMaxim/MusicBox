using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class StateBehaviour : StateMachineBehaviour
{
	public static StateBehaviour GetBehaviour(Animator _Animator, string _ID)
	{
		return _Animator != null ? _Animator.GetBehaviours<StateBehaviour>().FirstOrDefault(_State => string.Equals(_State.m_ID, _ID, StringComparison.InvariantCultureIgnoreCase)) : null;
	}

	public static StateBehaviour[] GetBehaviours(Animator _Animator, string _ID)
	{
		return _Animator != null ? _Animator.GetBehaviours<StateBehaviour>().Where(_State => string.Equals(_State.m_ID, _ID, StringComparison.InvariantCultureIgnoreCase)).ToArray() : null;
	}

	public event Action OnEnter;
	public event Action OnExit;
	public event Action OnComplete;

	[SerializeField] string m_ID;

	bool m_Complete;

	public override void OnStateEnter(Animator _Animator, AnimatorStateInfo _StateInfo, int _LayerIndex)
	{
		base.OnStateEnter(_Animator, _StateInfo, _LayerIndex);
		
		m_Complete = false;
		
		OnEnter?.Invoke();
	}

	public override void OnStateUpdate(Animator _Animator, AnimatorStateInfo _StateInfo, int _LayerIndex)
	{
		base.OnStateUpdate(_Animator, _StateInfo, _LayerIndex);
		
		if (!m_Complete && _StateInfo.normalizedTime >= 1)
		{
			m_Complete = true;
			OnComplete?.Invoke();
		}
	}

	public override void OnStateExit(Animator _Animator, AnimatorStateInfo _StateInfo, int _LayerIndex)
	{
		base.OnStateExit(_Animator, _StateInfo, _LayerIndex);
		
		if (!m_Complete)
		{
			m_Complete = true;
			OnComplete?.Invoke();
		}
		
		OnExit?.Invoke();
	}
}