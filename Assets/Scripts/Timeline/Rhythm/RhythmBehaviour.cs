using UnityEngine;
using UnityEngine.Playables;

public class RhythmBehaviour : TimelineBehaviour
{
	public RhythmIndicator RhythmIndicator { get; set; }

	public override void OnBehaviourEnter(Playable _Playable, FrameData _FrameData)
	{
		base.OnBehaviourEnter(_Playable, _FrameData);
		
		if (!Application.isPlaying)
			return;
		
		double duration = _Playable.GetDuration() - _Playable.GetTime();
		
		RhythmIndicator.Play((float)duration);
	}
}