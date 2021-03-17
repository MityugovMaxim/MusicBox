using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(InputTrack))]
public class InputTrackDrawer : TrackDrawer
{
	SerializedProperty InputReaderProperty { get; }
	SerializedProperty DurationProperty    { get; }
	SerializedProperty ZoneProperty        { get; }
	SerializedProperty ZoneMinProperty     { get; }
	SerializedProperty ZoneMaxProperty     { get; }

	public InputTrackDrawer(Track _Track) : base(_Track)
	{
		InputReaderProperty = TrackObject.FindProperty("m_InputReader");
		DurationProperty    = TrackObject.FindProperty("m_Duration");
		ZoneProperty        = TrackObject.FindProperty("m_Zone");
		ZoneMinProperty     = TrackObject.FindProperty("m_ZoneMin");
		ZoneMaxProperty     = TrackObject.FindProperty("m_ZoneMax");
	}

	protected override void DrawContent()
	{
		float duration = DurationProperty.floatValue;
		float zone     = ZoneProperty.floatValue;
		float zoneMin  = ZoneMinProperty.floatValue;
		float zoneMax  = ZoneMaxProperty.floatValue;
		
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			InputClip clip = ScriptableObject.CreateInstance<InputClip>();
			
			clip.name = "Input Clip";
			
			TrackUtility.AddClip(Track, clip, Time);
			
			Track.Initialize(Track.Sequencer);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(InputReaderProperty, GUIContent.none);
		
		EditorGUI.BeginChangeCheck();
		
		duration = EditorGUILayout.FloatField("Duration", duration);
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(EditorGUIUtility.fieldWidth + 8);
		zone = EditorGUILayout.Slider(GUIContent.none, zone, 0, 1);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		zoneMin = EditorGUILayout.FloatField(GUIContent.none, zoneMin, GUILayout.Width(EditorGUIUtility.fieldWidth));
		GUILayout.Space(2);
		EditorGUILayout.MinMaxSlider(GUIContent.none, ref zoneMin, ref zoneMax, 0, 1, GUILayout.MinWidth(10));
		GUILayout.Space(2);
		zoneMax = EditorGUILayout.FloatField(GUIContent.none, zoneMax, GUILayout.Width(EditorGUIUtility.fieldWidth));
		EditorGUILayout.EndHorizontal();
		
		if (EditorGUI.EndChangeCheck())
		{
			zone     = MathUtility.Snap(zone, 0.001f);
			zoneMin  = MathUtility.Snap(zoneMin, 0.001f);
			zoneMax  = MathUtility.Snap(zoneMax, 0.001f);
			duration = Mathf.Max(duration, 0.1f);
			
			DurationProperty.floatValue = duration;
			ZoneProperty.floatValue     = zone;
			ZoneMinProperty.floatValue  = zoneMin;
			ZoneMaxProperty.floatValue  = zoneMax;
			
			TrackObject.ApplyModifiedProperties();
			
			Track.Initialize(Track.Sequencer);
		}
	}
}