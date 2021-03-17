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
			
			TrackUtility.AddClip(Track, clip, Time, 0.2f);
			
			IRoutineClipReceiver[] receivers = Track.GetReferences<IRoutineClipReceiver>(TargetProperty.stringValue);
			
			clip.Initialize(Track.Sequencer, receivers);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(TargetProperty, GUIContent.none);
	}
}