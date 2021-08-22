using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(UIImage), true)]
[CanEditMultipleObjects]
public class UIImageEditor : Editor
{
	static readonly HashSet<string> m_ExceptProperties = new HashSet<string>()
	{
		"m_Material",
		"m_OnCullStateChanged",
	};

	public override void OnInspectorGUI()
	{
		serializedObject.UpdateIfRequiredOrScript();
		SerializedProperty iterator = serializedObject.GetIterator();
		for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
		{
			if (m_ExceptProperties.Contains(iterator.propertyPath))
				continue;
			
			using (new EditorGUI.DisabledScope(iterator.propertyPath == "m_Script"))
				EditorGUILayout.PropertyField(iterator, true);
		}
		serializedObject.ApplyModifiedProperties();
	}
}