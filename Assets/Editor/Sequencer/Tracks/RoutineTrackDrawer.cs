using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(RoutineTrack))]
public class RoutineTrackDrawer : TrackDrawer
{
	SerializedProperty TargetProperty { get; }

	public RoutineTrackDrawer(Track _Track) : base(_Track)
	{
		TargetProperty = TrackObject.FindProperty("m_Target");
	}

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			RoutineClip clip = ScriptableObject.CreateInstance<RoutineClip>();
			
			clip.name = "Routine Clip";
			
			using (SerializedObject clipObject = new SerializedObject(clip))
			{
				SerializedProperty minTimeProperty = clipObject.FindProperty("m_MinTime");
				SerializedProperty maxTimeProperty = clipObject.FindProperty("m_MaxTime");
				
				minTimeProperty.floatValue = Time;
				maxTimeProperty.floatValue = Time + 0.2f;
				
				clipObject.ApplyModifiedProperties();
			}
			
			AddClip(clip);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(TargetProperty, GUIContent.none);
	}
}