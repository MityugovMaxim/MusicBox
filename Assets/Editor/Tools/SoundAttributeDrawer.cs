using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SoundAttribute))]
public class SoundAttributeDrawer : PropertyDrawer
{
	public static bool Loaded { get; set; }

	static string[]     m_Sounds;
	static GUIContent[] m_Content;

	public override void OnGUI(Rect _Rect, SerializedProperty _Property, GUIContent _Label)
	{
		if (_Property.propertyType != SerializedPropertyType.String)
		{
			EditorGUI.PropertyField(_Rect, _Property, _Label);
			return;
		}
		
		EditorGUI.BeginChangeCheck();
		
		LoadSounds();
		
		int index = Array.FindIndex(m_Sounds, _SoundID => _SoundID == _Property.stringValue);
		
		Rect rect = new Rect(
			_Rect.x,
			_Rect.y + (_Rect.height - EditorGUIUtility.singleLineHeight) * 0.5f,
			_Rect.width,
			EditorGUIUtility.singleLineHeight
		);
		
		index = EditorGUI.Popup(rect, _Label, index, m_Content);
		
		if (EditorGUI.EndChangeCheck())
		{
			_Property.stringValue = m_Sounds[index];
			_Property.serializedObject.ApplyModifiedProperties();
		}
	}

	static void LoadSounds()
	{
		if (Loaded && m_Sounds != null && m_Sounds.Length != 0)
			return;
		
		Loaded = true;
		
		m_Sounds = AssetDatabase.FindAssets($"t:{nameof(SoundRegistry)}")
			.Select(AssetDatabase.GUIDToAssetPath)
			.Select(AssetDatabase.LoadAssetAtPath<SoundRegistry>)
			.Where(_Registry => _Registry != null)
			.SelectMany(_Registry => _Registry.ToArray())
			.Select(_SoundInfo => _SoundInfo.ID)
			.Distinct()
			.Prepend(string.Empty)
			.ToArray();
		
		m_Content = m_Sounds
			.Select(_SoundID => string.IsNullOrEmpty(_SoundID) ? new GUIContent("None") : new GUIContent(_SoundID))
			.ToArray();
	}
}