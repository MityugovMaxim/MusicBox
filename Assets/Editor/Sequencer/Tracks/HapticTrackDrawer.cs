using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(HapticTrack))]
public class HapticTrackDrawer : TrackDrawer
{
	public HapticTrackDrawer(Track _Track) : base(_Track) { }

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			HapticClip clip = ScriptableObject.CreateInstance<HapticClip>();
			
			clip.name = "Haptic Clip";
			
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
	}
}