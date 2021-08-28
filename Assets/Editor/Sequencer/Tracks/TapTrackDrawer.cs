using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(TapTrack))]
public class TapTrackDrawer : TrackDrawer
{
	SerializedProperty TrackProperty { get; }

	public TapTrackDrawer(Track _Track) : base(_Track)
	{
		TrackProperty = TrackObject.FindProperty("m_Track");
	}

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button(AddIconContent, AddButtonStyle))
		{
			TapClip clip = ScriptableObject.CreateInstance<TapClip>();
			
			clip.name = "Tap Clip";
			
			TrackUtility.AddClip(Track, clip, Time);
		}
		
		if (!string.IsNullOrEmpty(Track.Mnemonic) && Event.current.type == EventType.KeyDown && Event.current.character == Track.Mnemonic[0])
		{
			Event.current.Use();
			
			TapClip clip = ScriptableObject.CreateInstance<TapClip>();
			
			clip.name = "Tap Clip";
			
			TrackUtility.AddClip(Track, clip, Time);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(TrackProperty, GUIContent.none);
	}
}