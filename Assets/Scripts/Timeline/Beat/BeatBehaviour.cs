using System;
using UnityEngine;
using UnityEngine.Playables;

public class BeatBehaviour : TimelineBehaviour
{
	public BeatView BeatView { get; set; }

	public override void OnBehaviourEnter(Playable _Playable, FrameData _FrameData)
	{
		base.OnBehaviourEnter(_Playable, _FrameData);
		
		float duration = (float)(_Playable.GetDuration() - _Playable.GetTime());
		
		BeatView.Play(duration);
	}
}