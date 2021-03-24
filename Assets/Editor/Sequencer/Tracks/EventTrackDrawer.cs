using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(EventTrack))]
public class EventTrackDrawer : TrackDrawer
{
	SerializedProperty TargetProperty { get; }

	public EventTrackDrawer(Track _Track) : base(_Track)
	{
		TargetProperty = TrackObject.FindProperty("m_Target");
	}

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button(AddIconContent, AddButtonStyle))
		{
			EventClip clip = ScriptableObject.CreateInstance<EventClip>();
			
			clip.name = "Event Clip";
			
			TrackUtility.AddClip(Track, clip, Time);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(TargetProperty, GUIContent.none);
	}
}