using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(DoubleTrack))]
public class DoubleTrackDrawer : TrackDrawer
{
	SerializedProperty TrackProperty { get; }

	public DoubleTrackDrawer(Track _Track) : base(_Track)
	{
		TrackProperty = TrackObject.FindProperty("m_Track");
	}

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button(AddIconContent, AddButtonStyle))
		{
			DoubleClip clip = ScriptableObject.CreateInstance<DoubleClip>();
			
			clip.name = "Double Clip";
			
			TrackUtility.AddClip(Track, clip, Time);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(TrackProperty, GUIContent.none);
	}
}