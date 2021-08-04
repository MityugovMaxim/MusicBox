using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(Sequencer))]
public class SequencerEditor : Editor
{
	[NonSerialized] bool m_Initialized;

	ReorderableList m_TracksList;

	void OnEnable()
	{
		Initialize();
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		Initialize();
		
		if (m_TracksList != null)
			m_TracksList.DoLayoutList();
		
		serializedObject.ApplyModifiedProperties();
	}

	void Initialize()
	{
		if (m_Initialized)
			return;
		
		m_Initialized = true;
		
		SerializedProperty tracksProperty = serializedObject.FindProperty("m_Tracks");
		
		m_TracksList = new ReorderableList(serializedObject, tracksProperty, true, true, true, true);
		
		m_TracksList.drawHeaderCallback += _Rect =>
		{
			EditorGUI.DropShadowLabel(_Rect, "Tracks");
		};
		
		m_TracksList.drawElementCallback += (_Rect, _Index, _Active, _Focused) =>
		{
			Rect labelRect = new Rect(_Rect.x, _Rect.y, 20, _Rect.height);
			
			Rect trackRect = new Rect(_Rect.x + 20, _Rect.y, _Rect.width - 20, _Rect.height);
			
			trackRect = new RectOffset(1, 1, 1, 1).Remove(trackRect);
			
			SerializedProperty trackProperty = tracksProperty.GetArrayElementAtIndex(_Index);
			
			EditorGUI.LabelField(labelRect, _Index.ToString());
			
			EditorGUI.PropertyField(trackRect, trackProperty, GUIContent.none);
		};
	}
}