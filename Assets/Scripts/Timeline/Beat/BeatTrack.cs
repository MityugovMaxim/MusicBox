using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(BeatClip))]
[TrackBindingType(typeof(BeatView))]
public class BeatTrack : TrackAsset
{
	PlayableDirector m_Director;

	protected override Playable CreatePlayable(PlayableGraph _Graph, GameObject _GameObject, TimelineClip _Clip)
	{
		if (m_Director == null)
			m_Director = _GameObject.GetComponent<PlayableDirector>();
		
		if (_Clip.asset is BeatClip beatClip)
		{
			BeatView beatView = m_Director.GetGenericBinding(_Clip.parentTrack) as BeatView;
			
			beatClip.BeatView = new ExposedReference<BeatView>() { defaultValue = beatView };
		}
		
		return base.CreatePlayable(_Graph, _GameObject, _Clip);
	}
}