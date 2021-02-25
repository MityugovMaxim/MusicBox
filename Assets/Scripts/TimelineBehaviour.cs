using System;
using UnityEngine.Playables;

[Serializable]
public class TimelineBehaviour : PlayableBehaviour
{
	public override void OnBehaviourPlay(Playable _Playable, FrameData _FrameData)
	{
		base.OnBehaviourPlay(_Playable, _FrameData);
		
		if (_Playable.GetPlayState() != PlayState.Paused)
			OnBehaviourEnter(_Playable, _FrameData);
	}

	public override void OnBehaviourPause(Playable _Playable, FrameData _FrameData)
	{
		base.OnBehaviourPause(_Playable, _FrameData);
		
		if (_Playable.GetPlayState() != PlayState.Playing)
			OnBehaviourExit(_Playable, _FrameData);
	}

	public virtual void OnBehaviourEnter(Playable _Playable, FrameData _FrameData) { }

	public virtual void OnBehaviourExit(Playable _Playable, FrameData _FrameData) { }
}