using System;
using System.Linq;
using AudioBox.Logging;
using UnityEngine;

[Serializable]
public class StateBehaviour : StateMachineBehaviour
{
	static StateBehaviour GetBehaviour(Animator _Animator, string _ID)
	{
		return _Animator != null ? _Animator.GetBehaviours<StateBehaviour>().FirstOrDefault(_State => string.Equals(_State.m_ID, _ID, StringComparison.InvariantCultureIgnoreCase)) : null;
	}

	static StateBehaviour[] GetBehaviours(Animator _Animator, string _ID)
	{
		return _Animator != null ? _Animator.GetBehaviours<StateBehaviour>().Where(_State => string.Equals(_State.m_ID, _ID, StringComparison.InvariantCultureIgnoreCase)).ToArray() : null;
	}

	public static void RegisterComplete(Animator _Animator, string _ID, Action _Complete)
	{
		StateBehaviour[] states = GetBehaviours(_Animator, _ID);
		
		if (states == null || states.Length == 0)
		{
			Log.Warning(nameof(StateBehaviour), "Register complete failed. Animator: '{0}' ID: '{1}'.", _Animator.name, _ID);
			return;
		}
		
		foreach (StateBehaviour state in states)
		{
			if (state != null)
				state.OnComplete += _Complete;
		}
	}

	public static void UnregisterComplete(Animator _Animator, string _ID, Action _Complete)
	{
		StateBehaviour[] states = GetBehaviours(_Animator, _ID);
		
		if (states == null || states.Length == 0)
			return;
		
		foreach (StateBehaviour state in states)
		{
			if (state != null)
				state.OnComplete -= _Complete;
		}
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