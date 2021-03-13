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
		
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			EventClip clip = ScriptableObject.CreateInstance<EventClip>();
			
			clip.name = "Event Clip";
			
			using (SerializedObject clipObject = new SerializedObject(clip))
			{
				SerializedProperty minTimeProperty = clipObject.FindProperty("m_MinTime");
				SerializedProperty maxTimeProperty = clipObject.FindProperty("m_MaxTime");
				
				minTimeProperty.floatValue = Time;
				maxTimeProperty.floatValue = Time;
				
				clipObject.ApplyModifiedProperties();
			}
			
			AddClip(clip);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(TargetProperty, GUIContent.none);
	}
}