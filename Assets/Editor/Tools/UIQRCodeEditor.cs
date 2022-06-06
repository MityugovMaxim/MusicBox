using System.Linq;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(UIQRCode))]
public class UIQRCodeEditor : Editor
{
	SerializedProperty MessageProperty => m_MessageProperty ?? (m_MessageProperty = serializedObject.FindProperty("m_Message"));

	SerializedProperty m_MessageProperty;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		GUILayout.Space(20);
		
		EditorGUILayout.BeginHorizontal();
		
		DrawMessage();
		
		if (GUILayout.Button("Generate", GUILayout.Width(80)))
			Generate();
		
		EditorGUILayout.EndHorizontal();
	}

	void Generate()
	{
		UIQRCode[] qrCodes = targets.OfType<UIQRCode>().ToArray();
		
		if (qrCodes.Length == 0)
			return;
		
		foreach (UIQRCode qrCode in qrCodes)
			qrCode.Generate();
	}

	void DrawMessage()
	{
		string message = EditorGUILayout.DelayedTextField(MessageProperty.displayName, MessageProperty.stringValue);
		
		if (message == MessageProperty.stringValue)
			return;
		
		MessageProperty.stringValue = message;
		
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
		
		Generate();
	}
}