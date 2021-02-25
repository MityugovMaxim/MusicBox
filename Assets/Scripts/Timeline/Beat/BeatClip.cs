using UnityEngine;
using UnityEngine.Playables;

public class BeatClip : PlayableAsset
{
	public ExposedReference<BeatView> BeatView;

	public override Playable CreatePlayable(PlayableGraph _Graph, GameObject _Owner)
	{
		ScriptPlayable<BeatBehaviour> playable = ScriptPlayable<BeatBehaviour>.Create(_Graph);
		
		BeatBehaviour behaviour = playable.GetBehaviour();
		
		behaviour.BeatView = BeatView.Resolve(_Graph.GetResolver());
		
		return playable;
	}
}