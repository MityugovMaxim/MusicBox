using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(HoldTrack))]
public class HoldTrackDrawer : TrackDrawer
{
	SerializedProperty TrackProperty { get; }

	public HoldTrackDrawer(Track _Track) : base(_Track)
	{
		TrackProperty = TrackObject.FindProperty("m_Track");
	}

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button(AddIconContent, AddButtonStyle))
		{
			HoldClip clip = ScriptableObject.CreateInstance<HoldClip>();
			
			clip.name = "Hold Clip";
			clip.Curve.Add(new HoldCurve.Key(0, 0, Vector2.zero, Vector2.zero));
			clip.Curve.Add(new HoldCurve.Key(1, 0, Vector2.zero, Vector2.zero));
			clip.Curve.Reposition();
			
			TrackUtility.AddClip(Track, clip, Time, 0.2f);
		}
		
		if (!string.IsNullOrEmpty(Track.Mnemonic) && Event.current.type == EventType.KeyDown && Event.current.character == Track.Mnemonic[0])
		{
			Event.current.Use();
			
			HoldClip clip = ScriptableObject.CreateInstance<HoldClip>();
			
			clip.name = "Hold Clip";
			clip.Curve.Add(new HoldCurve.Key(0, 0, Vector2.zero, Vector2.zero));
			clip.Curve.Add(new HoldCurve.Key(1, 0, Vector2.zero, Vector2.zero));
			clip.Curve.Reposition();
			
			TrackUtility.AddClip(Track, clip, Time, 0.2f);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(TrackProperty, GUIContent.none);
	}
}