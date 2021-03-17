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
		DrawName();
		
		EditorGUILayout.PropertyField(AnimatorProperty, GUIContent.none);
	}
}