using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(CommandClip))]
[TrackBindingType(typeof(CommandView))]
public class CommandTrack : TrackAsset
{
	PlayableDirector m_Director;

	protected override Playable CreatePlayable(PlayableGraph _Graph, GameObject _GameObject, TimelineClip _Clip)
	{
		if (m_Director == null)
			m_Director = _GameObject.GetComponent<PlayableDirector>();
		
		if (_Clip.asset is CommandClip commandClip)
		{
			CommandView commandView = m_Director.GetGenericBinding(_Clip.parentTrack) as CommandView;
			
			commandClip.CommandView = new ExposedReference<CommandView>() { defaultValue = commandView };
		}
		
		return base.CreatePlayable(_Graph, _GameObject, _Clip);
	}
}