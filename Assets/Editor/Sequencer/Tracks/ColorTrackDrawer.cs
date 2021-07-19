using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(ColorTrack))]
public class ColorTrackDrawer : TrackDrawer
{
	SerializedProperty ColorProcessorProperty { get; }

	public ColorTrackDrawer(Track _Track) : base(_Track)
	{
		ColorProcessorProperty = TrackObject.FindProperty("m_ColorProcessor");
	}

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button(AddIconContent, AddButtonStyle))
		{
			ColorClip clip = ScriptableObject.CreateInstance<ColorClip>();
			
			clip.name = "Color Clip";
			
			TrackUtility.AddClip(Track, clip, Time, 0.2f);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(ColorProcessorProperty, GUIContent.none);
	}
}