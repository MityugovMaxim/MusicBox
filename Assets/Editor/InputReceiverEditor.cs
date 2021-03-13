using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(InputReceiver))]
public class InputReceiverEditor : Editor
{
	static readonly HashSet<string> m_ExcludedFields = new HashSet<string>()
	{
		"m_Material",
		"m_Color",
	};

	public override void OnInspectorGUI()
	{
		serializedObject.UpdateIfRequiredOrScript();
		
		SerializedProperty property = serializedObject.GetIterator();
		
		for (bool root = true; property.NextVisible(root); root = false)
		{
			if (m_ExcludedFields.Contains(property.propertyPath))
				continue;
			
			using (new EditorGUI.DisabledScope("m_Script" == property.propertyPath))
				EditorGUILayout.PropertyField(property, true);
		}
		
		serializedObject.ApplyModifiedProperties();
	}
}
