using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(Registry), true)]
public class RegistryEditor : Editor
{
	bool            m_Initialized;
	ReorderableList m_RegistryList;

	void OnEnable()
	{
		Initialize();
	}

	public override void OnInspectorGUI()
	{
		serializedObject.UpdateIfRequiredOrScript();
		
		Initialize();
		
		if (GUILayout.Button("Find all"))
			FindLevels();
		
		GUILayout.Space(10);
		
		if (m_RegistryList != null)
			m_RegistryList.DoLayoutList();
		
		serializedObject.ApplyModifiedProperties();
	}

	void Initialize()
	{
		if (m_Initialized)
			return;
		
		m_Initialized = true;
		
		Registry registry = target as Registry;
		
		if (registry == null)
			return;
		
		SerializedProperty levelInfosProperty = serializedObject.FindProperty("m_Registry");
		
		m_RegistryList = new ReorderableList(serializedObject, levelInfosProperty, true, true, true, true);
		
		m_RegistryList.drawHeaderCallback += _Rect =>
		{
			EditorGUI.DropShadowLabel(_Rect, registry.Name);
		};
		
		m_RegistryList.drawElementCallback += (_Rect, _Index, _Active, _Focused) =>
		{
			SerializedProperty levelInfoProperty = m_RegistryList.serializedProperty.GetArrayElementAtIndex(_Index);
			
			Rect indexRect = new Rect(_Rect.x, _Rect.y, 25, _Rect.height);
			Rect levelRect = new Rect(_Rect.x + 25, _Rect.y, _Rect.width - 25, _Rect.height);
			
			EditorGUI.LabelField(indexRect, _Index.ToString());
			EditorGUI.PropertyField(levelRect, levelInfoProperty, GUIContent.none);
		};
	}

	void FindLevels()
	{
		Registry registry = target as Registry;
		
		if (registry == null)
			return;
		
		List<ScriptableObject> entries = AssetDatabase.FindAssets($"t:{registry.AssetType.Name}")
			.Select(AssetDatabase.GUIDToAssetPath)
			.Select(AssetDatabase.LoadAssetAtPath<ScriptableObject>)
			.ToList();
		
		for (int i = entries.Count - 1; i >= 0; i--)
		{
			if (registry.Contains(entries[i]))
				entries.RemoveAt(i);
		}
		
		int index = m_RegistryList.serializedProperty.arraySize;
		for (var i = 0; i < entries.Count; i++)
		{
			ReorderableList.defaultBehaviours.DoAddButton(m_RegistryList);
			SerializedProperty entryProperty = m_RegistryList.serializedProperty.GetArrayElementAtIndex(index + i);
			entryProperty.objectReferenceValue = entries[i];
		}
	}
}