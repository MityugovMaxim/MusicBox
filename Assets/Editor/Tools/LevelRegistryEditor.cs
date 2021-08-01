using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(LevelRegistry))]
public class LevelRegistryEditor : Editor
{
	bool            m_Initialized;
	ReorderableList m_LevelInfosList;

	void OnEnable()
	{
		Initialize();
	}

	public override void OnInspectorGUI()
	{
		serializedObject.UpdateIfRequiredOrScript();
		
		Initialize();
		
		if (GUILayout.Button("Find levels"))
			FindLevels();
		
		if (m_LevelInfosList != null)
			m_LevelInfosList.DoLayoutList();
		
		serializedObject.ApplyModifiedProperties();
	}

	void Initialize()
	{
		if (m_Initialized)
			return;
		
		m_Initialized = true;
		
		SerializedProperty levelInfosProperty = serializedObject.FindProperty("m_LevelInfos");
		
		m_LevelInfosList = new ReorderableList(serializedObject, levelInfosProperty, true, true, true, true);
		
		m_LevelInfosList.drawHeaderCallback += _Rect =>
		{
			EditorGUI.DropShadowLabel(_Rect, "Levels");
		};
		
		m_LevelInfosList.drawElementCallback += (_Rect, _Index, _Active, _Focused) =>
		{
			SerializedProperty levelInfoProperty = m_LevelInfosList.serializedProperty.GetArrayElementAtIndex(_Index);
			
			Rect indexRect = new Rect(_Rect.x, _Rect.y, 25, _Rect.height);
			Rect levelRect = new Rect(_Rect.x + 25, _Rect.y, _Rect.width - 25, _Rect.height);
			
			EditorGUI.LabelField(indexRect, _Index.ToString());
			EditorGUI.PropertyField(levelRect, levelInfoProperty, GUIContent.none);
		};
	}

	void FindLevels()
	{
		LevelRegistry levelRegistry = target as LevelRegistry;
		
		if (levelRegistry == null)
			return;
		
		List<LevelInfo> levelInfos = AssetDatabase.FindAssets("t:LevelInfo")
			.Select(AssetDatabase.GUIDToAssetPath)
			.Select(AssetDatabase.LoadAssetAtPath<LevelInfo>)
			.ToList();
		
		for (int i = levelInfos.Count - 1; i >= 0; i--)
		{
			LevelInfo levelInfo = levelInfos[i];
			
			if (levelRegistry.Any(_LevelInfo => _LevelInfo.ID == levelInfo.ID))
				levelInfos.RemoveAt(i);
		}
		
		int index = m_LevelInfosList.serializedProperty.arraySize;
		for (var i = 0; i < levelInfos.Count; i++)
		{
			LevelInfo levelInfo = levelInfos[i];
			ReorderableList.defaultBehaviours.DoAddButton(m_LevelInfosList);
			SerializedProperty levelInfoProperty = m_LevelInfosList.serializedProperty.GetArrayElementAtIndex(index + i);
			levelInfoProperty.objectReferenceValue = levelInfo;
		}
	}
}