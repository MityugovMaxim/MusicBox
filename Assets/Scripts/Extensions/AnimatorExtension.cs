using System;
using UnityEngine;

public static class AnimatorExtension
{
	public static bool CheckState(this Animator _Animator, int _StateID)
	{
		AnimatorStateInfo source = _Animator.GetCurrentAnimatorStateInfo(0);
		
		if (source.shortNameHash == _StateID)
			return true;
		
		AnimatorStateInfo target = _Animator.GetNextAnimatorStateInfo(0);
		
		if (target.shortNameHash == _StateID)
			return true;
		
		return false;
	}

	public static void RegisterExit(this Animator _Animator, string _ID, Action _Action)
	{
		StateBehaviour.RegisterExit(_Animator, _ID, _Action);
	}

	public static void UnregisterExit(this Animator _Animator, string _ID, Action _Action)
	{
		StateBehaviour.UnregisterExit(_Animator, _ID, _Action);
	}

	public static void SubscribeComplete(this Animator _Animator, string _ID, Action _Action)
	{
		StateBehaviour.RegisterComplete(_Animator, _ID, _Action);
	}

	public static void UnsubscribeComplete(this Animator _Animator, string _ID, Action _Action)
	{
		StateBehaviour.UnregisterComplete(_Animator, _ID, _Action);
	}
}
