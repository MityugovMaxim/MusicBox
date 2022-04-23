using System.Linq;
using UnityEditor;

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