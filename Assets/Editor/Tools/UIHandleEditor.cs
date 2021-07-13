using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(UIHandle), true)]
public class UIHandleEditor : Editor
{
	static readonly HashSet<string> m_ExcludedFields = new HashSet<string>()
	{
		"m_Material",
		"m_Color",
		"m_RaycastTarget",
	};

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		serializedObject.UpdateIfRequiredOrScript();
		SerializedProperty iterator = serializedObject.GetIterator();
		
		iterator.NextVisible(true);
		
		do
		{
			if (m_ExcludedFields.Contains(iterator.propertyPath))
				continue;
			
			EditorGUILayout.PropertyField(iterator, true);
		}
		while (iterator.NextVisible(false));
		serializedObject.ApplyModifiedProperties();
	}
}