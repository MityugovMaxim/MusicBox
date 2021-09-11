using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MenuInfo))]
public class MenuInfoEditor : Editor
{
	SerializedProperty PathProperty => m_PathProperty ?? (m_PathProperty = serializedObject.FindProperty("m_Path"));
	SerializedProperty TypeProperty => m_TypeProperty ?? (m_TypeProperty = serializedObject.FindProperty("m_Type"));

	SerializedProperty m_PathProperty;
	SerializedProperty m_TypeProperty;

	UIMenu   m_Menu;
	MenuType m_Type;

	void OnEnable()
	{
		m_Menu = Resources.Load<UIMenu>(PathProperty.stringValue);
		
		if (m_Menu != null && MenuPrebuild.TryGetMenuType(m_Menu.GetType(), out MenuType type))
			m_Type = type;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		serializedObject.UpdateIfRequiredOrScript();
		
		EditorGUI.BeginChangeCheck();
		
		EditorGUILayout.PropertyField(PathProperty);
		
		if (EditorGUI.EndChangeCheck())
		{
			m_Menu = Resources.Load<UIMenu>(PathProperty.stringValue);
			if (m_Menu != null && MenuPrebuild.TryGetMenuType(m_Menu.GetType(), out MenuType type))
				m_Type = type;
		}
		
		if (TypeProperty.enumValueIndex != (int)m_Type)
			TypeProperty.enumValueIndex = (int)m_Type;
		
		GUI.enabled = false;
		EditorGUILayout.PropertyField(TypeProperty);
		GUI.enabled = true;
		
		serializedObject.ApplyModifiedProperties();
	}
}