using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HoldCurve))]
public class SplineCurveDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty _Property, GUIContent _Label)
	{
		return EditorGUIUtility.singleLineHeight + 80;
	}

	public override void OnGUI(Rect _Rect, SerializedProperty _Property, GUIContent _Label)
	{
		_Property.serializedObject.UpdateIfRequiredOrScript();
		
		HoldCurve holdCurve = fieldInfo.GetValue(_Property.serializedObject.targetObject) as HoldCurve;
		
		Rect labelRect = new Rect(
			_Rect.x,
			_Rect.y,
			_Rect.width,
			EditorGUIUtility.singleLineHeight
		);
		
		Rect splineRect = new Rect(
			_Rect.x,
			_Rect.y + EditorGUIUtility.singleLineHeight,
			_Rect.width,
			_Rect.height - EditorGUIUtility.singleLineHeight
		);
		
		int controlID = EditorGUIUtility.GetControlID(FocusType.Passive);
		EditorGUI.PrefixLabel(labelRect, controlID, _Label);
		
		GUIHoldCurve.DrawSpline(splineRect, holdCurve);
		
		EditorUtility.SetDirty(_Property.serializedObject.targetObject);
	}
}