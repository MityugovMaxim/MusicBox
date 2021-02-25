using UnityEngine;
using UnityEngine.Playables;

public class CommandClip : PlayableAsset
{
	public ExposedReference<CommandView> CommandView;
	public CommandType                   CommandType;

	public override Playable CreatePlayable(PlayableGraph _Graph, GameObject _Owner)
	{
		ScriptPlayable<CommandBehaviour> playable = ScriptPlayable<CommandBehaviour>.Create(_Graph);
		
		CommandBehaviour behaviour = playable.GetBehaviour();
		
		behaviour.CommandView = CommandView.Resolve(_Graph.GetResolver());
		behaviour.CommandType = CommandType;
		
		return playable;
	}
}