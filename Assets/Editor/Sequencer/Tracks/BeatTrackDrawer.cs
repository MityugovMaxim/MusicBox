using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(BeatTrack))]
public class BeatTrackDrawer : TrackDrawer
{
	SerializedProperty TrackProperty { get; }

	public BeatTrackDrawer(Track _Track) : base(_Track)
	{
		TrackProperty = TrackObject.FindProperty("m_Track");
	}

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(TrackProperty, GUIContent.none);
	}
}