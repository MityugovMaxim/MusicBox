using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(UILocalizedLabel))]
public class UILocalizedLabelEditor : Editor
{
	SerializedProperty KeyProperty => m_KeyProperty ?? (m_KeyProperty = serializedObject.FindProperty("m_Key"));

	SerializedProperty m_KeyProperty;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		DrawKey();
	}

	void DrawKey()
	{
		string key = EditorGUILayout.DelayedTextField(KeyProperty.displayName, KeyProperty.stringValue);
		
		if (key == KeyProperty.stringValue)
			return;
		
		StringBuilder wordBuilder = new StringBuilder();
		
		List<string> words = new List<string>();
		
		foreach (char symbol in key)
		{
			if (char.IsLetterOrDigit(symbol))
			{
				wordBuilder.Append(symbol);
			}
			else if (wordBuilder.Length > 0)
			{
				words.Add(wordBuilder.ToString());
				wordBuilder.Clear();
			}
		}
		
		key = string.Join("_", words.Select(_Word => _Word.ToUpperInvariant()));
		
		if (key == KeyProperty.stringValue)
			return;
		
		KeyProperty.stringValue = key;
	}
}

[CanEditMultipleObjects]
[CustomEditor(typeof(WebImage))]
public class WebImageEditor : Editor
{
	SerializedProperty PathProperty => m_PathProperty ?? (m_PathProperty = serializedObject.FindProperty("m_Path"));

	SerializedProperty m_PathProperty;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		DrawPath();
	}

	void DrawPath()
	{
		string path = EditorGUILayout.DelayedTextField(PathProperty.displayName, PathProperty.stringValue);
		
		if (path == PathProperty.stringValue)
			return;
		
		PathProperty.stringValue = path;
		
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
		
		Reload();
	}

	void Reload()
	{
		WebImage[] images = targets.OfType<WebImage>().ToArray();
		
		foreach (WebImage image in images)
			image.Reload();
	}
}