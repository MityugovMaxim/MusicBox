using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(AlphaTrack))]
public class AlphaTrackDrawer : TrackDrawer
{
	SerializedProperty CanvasGroupProperty { get; }

	public AlphaTrackDrawer(Track _Track) : base(_Track)
	{
		CanvasGroupProperty = TrackObject.FindProperty("m_CanvasGroup");
	}

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button(AddIconContent, AddButtonStyle))
		{
			AlphaClip clip = ScriptableObject.CreateInstance<AlphaClip>();
			
			clip.name = "Alpha Clip";
			
			TrackUtility.AddClip(Track, clip, Time);
		}
		
		if (!string.IsNullOrEmpty(Track.Mnemonic) && Event.current.type == EventType.KeyDown && Event.current.character == Track.Mnemonic[0])
		{
			Event.current.Use();
			
			AlphaClip clip = ScriptableObject.CreateInstance<AlphaClip>();
			
			clip.name = "Alpha Clip";
			
			TrackUtility.AddClip(Track, clip, Time);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(CanvasGroupProperty, GUIContent.none);
	}
}