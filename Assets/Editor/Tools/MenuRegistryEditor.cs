using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(MenuRegistry), true)]
public class MenuRegistryEditor : Editor
{
	bool            m_Initialized;
	ReorderableList m_RegistryList;
	Editor          m_EntryEditor;

	void OnEnable()
	{
		Initialize();
	}

	public override void OnInspectorGUI()
	{
		serializedObject.UpdateIfRequiredOrScript();
		
		Initialize();
		
		if (GUILayout.Button("Find all"))
			FindAll();
		
		GUILayout.Space(10);
		
		if (m_RegistryList != null)
			m_RegistryList.DoLayoutList();
		
		serializedObject.ApplyModifiedProperties();
		
		if (m_EntryEditor != null)
			m_EntryEditor.OnInspectorGUI();
	}

	void Initialize()
	{
		if (m_Initialized)
			return;
		
		m_Initialized = true;
		
		MenuRegistry registry = target as MenuRegistry;
		
		if (registry == null)
			return;
		
		SerializedProperty registryProperty = serializedObject.FindProperty("m_Registry");
		
		m_RegistryList = new ReorderableList(serializedObject, registryProperty, true, true, true, true);
		
		m_RegistryList.drawHeaderCallback += _Rect =>
		{
			EditorGUI.DropShadowLabel(_Rect, "Menus");
		};
		
		m_RegistryList.drawElementCallback += (_Rect, _Index, _Active, _Focused) =>
		{
			Rect indexRect = new Rect(_Rect.x, _Rect.y, 25, _Rect.height);
			Rect entryRect = new Rect(_Rect.x + 25, _Rect.y, _Rect.width - 25, _Rect.height);
			
			EditorGUI.LabelField(indexRect, _Index.ToString());
			
			SerializedProperty entryProperty = m_RegistryList.serializedProperty.GetArrayElementAtIndex(_Index);
			
			if (entryProperty == null)
				return;
			
			EditorGUI.PropertyField(entryRect, entryProperty, GUIContent.none);
			
			MenuInfo menuInfo = entryProperty.objectReferenceValue as MenuInfo;
			
			if (menuInfo != null && menuInfo.Focusable)
				EditorGUI.DrawRect(new Rect(_Rect.x + 20, _Rect.y + 2, 2, _Rect.height - 4), new Color(0.25f, 1, 0.5f));
		};
		
		m_RegistryList.onSelectCallback += _List =>
		{
			int index = _List.index;
			
			if (index < 0 || index >= _List.count)
			{
				m_EntryEditor = null;
				return;
			}
			
			SerializedProperty entryProperty = m_RegistryList.serializedProperty.GetArrayElementAtIndex(index);
			
			if (entryProperty == null || entryProperty.objectReferenceValue == null)
			{
				m_EntryEditor = null;
				return;
			}
			
			m_EntryEditor = Editor.CreateEditor(entryProperty.objectReferenceValue);
		};
	}

	void FindAll()
	{
		MenuRegistry registry = target as MenuRegistry;
		
		if (registry == null)
			return;
		
		List<ScriptableObject> entries = AssetDatabase.FindAssets($"t:{nameof(MenuInfo)}")
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
