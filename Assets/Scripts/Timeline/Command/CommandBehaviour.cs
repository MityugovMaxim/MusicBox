using UnityEngine;
using UnityEngine.Playables;

public class CommandBehaviour : TimelineBehaviour
{
	public CommandView CommandView { get; set; }
	public CommandType CommandType { get; set; }

	public override void OnBehaviourEnter(Playable _Playable, FrameData _FrameData)
	{
		base.OnBehaviourEnter(_Playable, _FrameData);
		
		if (!Application.isPlaying)
			return;
		
		float duration = (float)(_Playable.GetDuration() - _Playable.GetTime());
		
		CommandView.Show(duration, CommandType);
	}
}