using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(MotionTrack))]
public class MotionTrackDrawer : TrackDrawer
{
	SerializedProperty AnimatorProperty { get; }

	public MotionTrackDrawer(Track _Track) : base(_Track)
	{
		AnimatorProperty = TrackObject.FindProperty("m_Animator");
	}

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			MotionClip clip = ScriptableObject.CreateInstance<MotionClip>();
			
			clip.name = "Motion Clip";
			
			TrackUtility.AddClip(Track, clip, Time, 0.2f);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(AnimatorProperty, GUIContent.none);
	}
}