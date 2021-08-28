using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(GotoTrack))]
public class GotoTrackDrawer : TrackDrawer
{
	SerializedProperty ResolverProperty { get; }

	public GotoTrackDrawer(Track _Track) : base(_Track)
	{
		ResolverProperty = TrackObject.FindProperty("m_Resolver");
	}

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button(AddIconContent, AddButtonStyle))
		{
			GotoClip clip = ScriptableObject.CreateInstance<GotoClip>();
			
			clip.name = "Goto Clip";
			
			TrackUtility.AddClip(Track, clip, Time, 0.2f);
		}
		
		if (!string.IsNullOrEmpty(Track.Mnemonic) && Event.current.type == EventType.KeyDown && Event.current.character == Track.Mnemonic[0])
		{
			Event.current.Use();
			
			GotoClip clip = ScriptableObject.CreateInstance<GotoClip>();
			
			clip.name = "Goto Clip";
			
			TrackUtility.AddClip(Track, clip, Time, 0.2f);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(ResolverProperty, GUIContent.none);
	}
}
