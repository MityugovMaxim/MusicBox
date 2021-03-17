using System.Linq;
using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(InputTrack))]
public class InputTrackDrawer : TrackDrawer
{
	SerializedProperty InputReaderProperty { get; }
	SerializedProperty DurationProperty    { get; }
	SerializedProperty TimeProperty        { get; }
	SerializedProperty MinZoneProperty     { get; }
	SerializedProperty MaxZoneProperty     { get; }

	public InputTrackDrawer(Track _Track) : base(_Track)
	{
		InputReaderProperty = TrackObject.FindProperty("m_InputReader");
		DurationProperty    = TrackObject.FindProperty("m_Duration");
		TimeProperty        = TrackObject.FindProperty("m_Time");
		MinZoneProperty     = TrackObject.FindProperty("m_MinZone");
		MaxZoneProperty     = TrackObject.FindProperty("m_MaxZone");
	}

	protected override void DrawContent()
	{
		float duration = DurationProperty.floatValue;
		float time     = TimeProperty.floatValue;
		float minZone  = MinZoneProperty.floatValue;
		float maxZone  = MaxZoneProperty.floatValue;
		
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			InputClip clip = ScriptableObject.CreateInstance<InputClip>();
			
			clip.name = "Input Clip";
			
			TrackUtility.AddClip(Track, clip, Time);
			
			Track.Initialize(Track.Sequencer);
			
			clip.Setup(duration, time, minZone, maxZone);
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(InputReaderProperty, GUIContent.none);
		
		EditorGUI.BeginChangeCheck();
		
		duration = EditorGUILayout.FloatField("Duration", duration);
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(EditorGUIUtility.fieldWidth + 8);
		time = EditorGUILayout.Slider(GUIContent.none, time, 0, 1);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		minZone = EditorGUILayout.FloatField(GUIContent.none, minZone, GUILayout.Width(EditorGUIUtility.fieldWidth));
		GUILayout.Space(2);
		EditorGUILayout.MinMaxSlider(GUIContent.none, ref minZone, ref maxZone, 0, 1, GUILayout.MinWidth(10));
		GUILayout.Space(2);
		maxZone = EditorGUILayout.FloatField(GUIContent.none, maxZone, GUILayout.Width(EditorGUIUtility.fieldWidth));
		EditorGUILayout.EndHorizontal();
		
		if (EditorGUI.EndChangeCheck())
		{
			time    = MathUtility.Snap(time, 0.001f);
			minZone = MathUtility.Snap(minZone, 0.001f);
			maxZone = MathUtility.Snap(maxZone, 0.001f);
			
			DurationProperty.floatValue = duration;
			TimeProperty.floatValue     = time;
			MinZoneProperty.floatValue  = minZone;
			MaxZoneProperty.floatValue  = maxZone;
			
			TrackObject.ApplyModifiedProperties();
			
			foreach (InputClip clip in Track.OfType<InputClip>())
				clip.Setup(duration, time, minZone, maxZone);
		}
	}
}