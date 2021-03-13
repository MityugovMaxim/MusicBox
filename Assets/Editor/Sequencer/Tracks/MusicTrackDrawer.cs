using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(MusicTrack))]
public class MusicTrackDrawer : TrackDrawer
{
	SerializedProperty AudioSourceProperty { get; }

	public MusicTrackDrawer(Track _Track) : base(_Track)
	{
		AudioSourceProperty = TrackObject.FindProperty("m_AudioSource");
	}

	protected override void DrawContent()
	{
		DrawName();
		
		EditorGUILayout.PropertyField(AudioSourceProperty, GUIContent.none);
	}
}