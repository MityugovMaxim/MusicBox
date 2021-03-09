using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[SequencerDrawer(typeof(MusicTrack))]
public class MusicTrackDrawer : TrackDrawer
{
	SerializedProperty AudioSourceProperty { get; }

	public MusicTrackDrawer(Track _Track) : base(_Track)
	{
		AudioSourceProperty = TrackObject.FindProperty("m_AudioSource");
	}

	protected override void DrawContent()
	{
		DrawName();
		
		EditorGUILayout.PropertyField(AudioSourceProperty, GUIContent.none);
	}
}

[SequencerDrawer(typeof(EventTrack))]
public class EventTrackDrawer : TrackDrawer
{
	public EventTrackDrawer(Track _Track) : base(_Track) { }

	protected override void DrawContent()
	{
		EditorGUILayout.BeginHorizontal();
		
		DrawName();
		
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			EventClip clip = ScriptableObject.CreateInstance<EventClip>();
			
			clip.name = "EventClip";
			
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

public class TrackDrawer
{
	static readonly Dictionary<Type, Type> m_TrackDrawerTypes = new Dictionary<Type, Type>();

	public static TrackDrawer Create(Track _Track)
	{
		if (_Track == null)
			return null;
		
		Type trackType = _Track.GetType();
		
		Type trackDrawerType = GetClipDrawerType(trackType);
		
		return Activator.CreateInstance(
			trackDrawerType,
			new object[] { _Track }
		) as TrackDrawer;
	}

	static Type GetClipDrawerType(Type _TrackType)
	{
		if (m_TrackDrawerTypes.ContainsKey(_TrackType) && m_TrackDrawerTypes[_TrackType] != null)
			return m_TrackDrawerTypes[_TrackType];
		
		Assembly assembly = typeof(TrackDrawer).Assembly;
		
		IEnumerable<Type> trackDrawerTypes = assembly.GetTypes().Where(_Type => _Type.IsSubclassOf(typeof(TrackDrawer)));
		
		foreach (Type trackDrawerType in trackDrawerTypes)
		{
			SequencerDrawerAttribute attribute = trackDrawerType.GetCustomAttribute<SequencerDrawerAttribute>();
			
			if (attribute.Type == _TrackType)
			{
				m_TrackDrawerTypes[_TrackType] = trackDrawerType;
				
				return trackDrawerType;
			}
		}
		
		return typeof(TrackDrawer);
	}

	protected Track            Track       { get; }
	protected SerializedObject TrackObject { get; }

	protected Rect  TrackRect   { get; private set; }
	protected Rect  ContentRect { get; private set; }
	protected float Time        { get; private set; }

	public TrackDrawer(Track _Track)
	{
		Track       = _Track;
		TrackObject = new SerializedObject(_Track);
	}

	public void Draw(Rect _TrackRect, float _Time)
	{
		TrackRect = _TrackRect;
		Time      = _Time;
		
		ContentRect = new RectOffset(15, 15, 4, 4).Remove(TrackRect);
		
		DrawBackground();
		
		GUILayout.BeginArea(ContentRect);
		
		DrawContent();
		
		GUILayout.EndArea();
	}

	protected void DrawName()
	{
		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
			GUI.FocusControl(null);
		
		string trackName = EditorGUILayout.DelayedTextField(Track.name, EditorStyles.boldLabel);
		
		if (Track.name == trackName)
			return;
		
		string path = AssetDatabase.GetAssetPath(Track);
		
		Track.name = trackName;
		
		AssetDatabase.RenameAsset(path, trackName);
		AssetDatabase.ImportAsset(path);
	}

	protected virtual void DrawBackground()
	{
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				EditorGUI.DrawRect(TrackRect, new Color(0.12f, 0.12f, 0.12f));
				
				AudioCurveRendering.DrawCurveFrame(TrackRect);
				
				break;
			}
		}
	}

	protected virtual void DrawContent()
	{
		DrawName();
	}

	protected virtual void DrawHandles()
	{
		
	}

	protected void AddClip<T>(T _Clip) where T : Clip
	{
		TrackObject.UpdateIfRequiredOrScript();
		
		AssetDatabase.AddObjectToAsset(_Clip, Track);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		SerializedProperty clipsProperty = TrackObject.FindProperty("m_Clips");
		
		int index = clipsProperty.arraySize;
		
		clipsProperty.InsertArrayElementAtIndex(index);
		
		TrackObject.ApplyModifiedProperties();
		TrackObject.UpdateIfRequiredOrScript();
		
		SerializedProperty clipProperty = clipsProperty.GetArrayElementAtIndex(index);
		
		clipProperty.objectReferenceValue = _Clip;
		
		TrackObject.ApplyModifiedProperties();
	}
}