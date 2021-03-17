using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(HapticTrack))]
public class HapticTrackDrawer : TrackDrawer
{
	public HapticTrackDrawer(Track _Track) : base(_Track) { }

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			HapticClip clip = ScriptableObject.CreateInstance<HapticClip>();
			
			clip.name = "Haptic Clip";
			
			TrackUtility.AddClip(Track, clip, Time);
			
			clip.Initialize(Track.Sequencer);
		}
		
		EditorGUILayout.EndHorizontal();
	}
}