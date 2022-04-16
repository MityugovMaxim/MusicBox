using System;
using UnityEngine;

public static class AnimatorExtension
{
	public static void RegisterComplete(this Animator _Animator, string _ID, Action _Action)
	{
		StateBehaviour.RegisterComplete(_Animator, _ID, _Action);
	}

	public static void UnregisterComplete(this Animator _Animator, string _ID, Action _Action)
	{
		StateBehaviour.UnregisterComplete(_Animator, _ID, _Action);
	}
}