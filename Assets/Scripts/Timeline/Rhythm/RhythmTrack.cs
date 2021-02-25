using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(RhythmClip))]
[TrackBindingType(typeof(RhythmIndicator))]
public class RhythmTrack : TrackAsset
{
	PlayableDirector m_Director;

	protected override Playable CreatePlayable(PlayableGraph _Graph, GameObject _GameObject, TimelineClip _Clip)
	{
		if (m_Director == null)
			m_Director = _GameObject.GetComponent<PlayableDirector>();
		
		if (_Clip.asset is RhythmClip rhythmClip)
		{
			RhythmIndicator rhythmIndicator = m_Director.GetGenericBinding(_Clip.parentTrack) as RhythmIndicator;
			
			rhythmClip.RhythmIndicator = new ExposedReference<RhythmIndicator>() { defaultValue = rhythmIndicator };
		}
		
		return base.CreatePlayable(_Graph, _GameObject, _Clip);
	}
}