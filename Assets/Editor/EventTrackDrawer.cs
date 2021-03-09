using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(EventTrack))]
public class EventTrackDrawer : TrackDrawer
{
	SerializedProperty ComponentProperty { get; }

	public EventTrackDrawer(Track _Track) : base(_Track)
	{
		ComponentProperty = TrackObject.FindProperty("m_Component");
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
		
		EditorGUILayout.PropertyField(ComponentProperty, GUIContent.none);
	}
}