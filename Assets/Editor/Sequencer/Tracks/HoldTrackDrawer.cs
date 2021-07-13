using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(HoldTrack))]
public class HoldTrackDrawer : TrackDrawer
{
	SerializedProperty TrackProperty { get; }

	public HoldTrackDrawer(Track _Track) : base(_Track)
	{
		TrackProperty = TrackObject.FindProperty("m_Track");
	}

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button(AddIconContent, AddButtonStyle))
		{
			HoldClip clip = ScriptableObject.CreateInstance<HoldClip>();
			
			clip.name = "Hold Clip";
			
			TrackUtility.AddClip(Track, clip, Time, 0.2f);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(TrackProperty, GUIContent.none);
	}
}