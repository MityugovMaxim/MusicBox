using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(PathAttribute))]
public class PathAttributeDrawer : PropertyDrawer
{
	static readonly Dictionary<string, Object> m_Cache = new Dictionary<string, Object>();

	public override void OnGUI(Rect _Rect, SerializedProperty _Property, GUIContent _Label)
	{
		if (_Property.propertyType != SerializedPropertyType.String)
		{
			EditorGUI.PropertyField(_Rect, _Property, _Label);
			return;
		}
		
		PathAttribute pathAttribute = attribute as PathAttribute;
		
		if (pathAttribute == null)
		{
			EditorGUI.PropertyField(_Rect, _Property, _Label);
			return;
		}
		
		EditorGUI.BeginChangeCheck();
		
		Object asset = EditorGUI.ObjectField(
			new Rect(
				_Rect.x,
				_Rect.y + (_Rect.height - EditorGUIUtility.singleLineHeight) * 0.5f,
				_Rect.width,
				EditorGUIUtility.singleLineHeight
			),
			_Label,
			GetAsset(pathAttribute.Type, _Property.stringValue),
			pathAttribute.Type,
			true
		);
		
		if (EditorGUI.EndChangeCheck())
		{
			_Property.stringValue = GetPath(asset);
			_Property.serializedObject.ApplyModifiedProperties();
		}
	}

	static Object GetAsset(Type _Type, string _Path)
	{
		if (m_Cache.ContainsKey(_Path) && m_Cache[_Path] != null)
			return m_Cache[_Path];
		
		Object asset = Resources.Load(_Path, _Type);
		
		m_Cache[_Path] = asset;
		
		return asset;
	}

	static string GetPath(Object _Asset)
	{
		const string directory = "/Resources/";
		
		if (_Asset == null)
			return null;
		
		string path = AssetDatabase.GetAssetPath(_Asset);
		
		int directoryIndex = path.IndexOf(directory, StringComparison.InvariantCulture);
		
		if (directoryIndex < 0)
			return null;
		
		int extensionIndex = path.LastIndexOf('.');
		
		if (extensionIndex < 0)
			extensionIndex = path.Length;
		
		return path.Substring(
			directoryIndex + directory.Length,
			extensionIndex - directoryIndex - directory.Length
		);
	}
}