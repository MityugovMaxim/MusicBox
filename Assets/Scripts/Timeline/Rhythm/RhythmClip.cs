using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class RhythmClip : PlayableAsset, ITimelineClipAsset
{
	public ExposedReference<RhythmIndicator> RhythmIndicator;

	public override Playable CreatePlayable(PlayableGraph _Graph, GameObject _GameObject)
	{
		RhythmBehaviour behaviour = new RhythmBehaviour();
		
		behaviour.RhythmIndicator = RhythmIndicator.Resolve(_Graph.GetResolver());
		
		return ScriptPlayable<RhythmBehaviour>.Create(_Graph, behaviour);
	}

	public ClipCaps clipCaps => ClipCaps.All;
}