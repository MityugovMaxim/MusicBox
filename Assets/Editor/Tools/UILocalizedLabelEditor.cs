using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(UILocalizedLabel))]
public class UILocalizedLabelEditor : Editor
{
	SerializedProperty KeyProperty     => m_KeyProperty ?? (m_KeyProperty = serializedObject.FindProperty("m_Key"));
	SerializedProperty OptionsProperty => m_OptionsProperty ?? (m_OptionsProperty = serializedObject.FindProperty("m_Options"));
	SerializedProperty DataProperty    => m_DataProperty ?? (m_DataProperty = serializedObject.FindProperty("m_Data"));
	SerializedProperty PrefixProperty  => m_PrefixProperty ?? (m_PrefixProperty = serializedObject.FindProperty("m_Prefix"));
	SerializedProperty PostfixProperty => m_PostfixProperty ?? (m_PostfixProperty = serializedObject.FindProperty("m_Postfix"));

	SerializedProperty m_KeyProperty;
	SerializedProperty m_OptionsProperty;
	SerializedProperty m_DataProperty;
	SerializedProperty m_PrefixProperty;
	SerializedProperty m_PostfixProperty;

	ReorderableList m_DataList;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		DrawKey();
		
		DrawOptions();
		
		serializedObject.ApplyModifiedProperties();
	}

	void DrawKey()
	{
		string key = EditorGUILayout.DelayedTextField(KeyProperty.displayName, KeyProperty.stringValue);
		
		if (key == KeyProperty.stringValue)
			return;
		
		key = key.ToAllCapital();
		
		if (key == KeyProperty.stringValue)
			return;
		
		KeyProperty.stringValue = key;
	}

	void DrawOptions()
	{
		EditorGUILayout.PropertyField(OptionsProperty);
		
		UILocalizedLabel.Options options = (UILocalizedLabel.Options)OptionsProperty.intValue;
		
		if (UILocalizedLabel.CheckOptions(options, UILocalizedLabel.Options.Format))
			DrawData();
		
		if (UILocalizedLabel.CheckOptions(options, UILocalizedLabel.Options.Prefix))
			DrawPrefix();
		
		if (UILocalizedLabel.CheckOptions(options, UILocalizedLabel.Options.Postfix))
			DrawPostfix();
	}

	void DrawData()
	{
		if (m_DataList == null)
			m_DataList = CreateDataList();
		else
			m_DataList.DoLayoutList();
	}

	void DrawPostfix()
	{
		EditorGUILayout.PropertyField(PostfixProperty);
	}

	void DrawPrefix()
	{
		EditorGUILayout.PropertyField(PrefixProperty);
	}

	ReorderableList CreateDataList()
	{
		ReorderableList list = new ReorderableList(serializedObject, DataProperty, true, true, true, true);
		
		list.drawHeaderCallback += _Rect =>
		{
			EditorGUI.SelectableLabel(_Rect, DataProperty.displayName);
		};
		
		list.drawElementCallback += (_Rect, _Index, _Active, _Focused) =>
		{
			Rect rect = new RectOffset(0, 0, 1, 1).Remove(_Rect);
			
			EditorGUI.PropertyField(rect, list.serializedProperty.GetArrayElementAtIndex(_Index));
		};
		
		return list;
	}
}