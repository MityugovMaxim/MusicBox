using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(RemoteImage), true)]
public class RemoteImageEditor : Editor
{
	SerializedProperty OptionsProperty   => m_OptionsProperty ?? (m_OptionsProperty = serializedObject.FindProperty("m_Options"));
	SerializedProperty PathProperty      => m_PathProperty ?? (m_PathProperty = serializedObject.FindProperty("m_Path"));
	SerializedProperty AtlasIDProperty   => m_AtlasIDProperty ?? (m_AtlasIDProperty = serializedObject.FindProperty("m_AtlasID"));
	SerializedProperty AtlasSizeProperty => m_AtlasSizeProperty ?? (m_AtlasSizeProperty = serializedObject.FindProperty("m_AtlasSize"));
	SerializedProperty WidthProperty     => m_WidthProperty ?? (m_WidthProperty = serializedObject.FindProperty("m_Width"));
	SerializedProperty HeightProperty    => m_HeightProperty ?? (m_HeightProperty = serializedObject.FindProperty("m_Height"));

	SerializedProperty m_OptionsProperty;
	SerializedProperty m_PathProperty;
	SerializedProperty m_AtlasIDProperty;
	SerializedProperty m_AtlasSizeProperty;
	SerializedProperty m_WidthProperty;
	SerializedProperty m_HeightProperty;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		EditorGUILayout.Space(20);
		
		serializedObject.UpdateIfRequiredOrScript();
		
		DrawPath();
		
		DrawOptions();
		
		DrawAtlas();
		
		serializedObject.ApplyModifiedProperties();
	}

	void DrawOptions()
	{
		EditorGUILayout.PropertyField(OptionsProperty);
	}

	void DrawPath()
	{
		string path = EditorGUILayout.DelayedTextField(PathProperty.displayName, PathProperty.stringValue);
		
		if (path == PathProperty.stringValue)
			return;
		
		PathProperty.stringValue = path;
		
		if (target is RemoteImage image)
			image.SetSpriteAsync(path);
	}

	void DrawAtlas()
	{
		if (!CheckOptions(RemoteImage.Options.Pack))
			return;
		
		EditorGUILayout.PropertyField(AtlasIDProperty);
		EditorGUILayout.PropertyField(AtlasSizeProperty);
		EditorGUILayout.PropertyField(WidthProperty);
		EditorGUILayout.PropertyField(HeightProperty);
		
		RemoteImage image = target as RemoteImage;
		
		if (image == null || image.Sprite == null || image.Sprite.texture == null)
			return;
		
		Rect rect = GUILayoutUtility.GetRect(float.MinValue, float.MaxValue, 200, 200);
		
		rect = MathUtility.Fit(rect, 1, new Vector2(0.5f, 0.5f));
		
		GUI.DrawTexture(rect, image.Sprite.texture);
	}

	bool CheckOptions(RemoteImage.Options _Options)
	{
		RemoteImage.Options options = (RemoteImage.Options)OptionsProperty.intValue;
		
		return (options & _Options) == _Options;
	}
}