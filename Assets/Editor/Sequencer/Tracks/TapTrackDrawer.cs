using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(TapTrack))]
public class TapTrackDrawer : TrackDrawer
{
	SerializedProperty TrackProperty { get; }

	public TapTrackDrawer(Track _Track) : base(_Track)
	{
		TrackProperty = TrackObject.FindProperty("m_Track");
	}

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button(AddIconContent, AddButtonStyle))
		{
			TapClip clip = ScriptableObject.CreateInstance<TapClip>();
			
			clip.name = "Tap Clip";
			
			TrackUtility.AddClip(Track, clip, Time);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(TrackProperty, GUIContent.none);
	}
}