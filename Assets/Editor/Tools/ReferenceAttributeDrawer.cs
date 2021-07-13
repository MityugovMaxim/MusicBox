using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(ReferenceAttribute))]
public class ReferenceAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect _Rect, SerializedProperty _Property, GUIContent _Label)
	{
		if (_Property.propertyType != SerializedPropertyType.String)
		{
			EditorGUI.PropertyField(_Rect, _Property, _Label);
			return;
		}
		
		IReferenceResolver resolver  = _Property.serializedObject.targetObject as IReferenceResolver;
		ReferenceAttribute reference = attribute as ReferenceAttribute;
		
		if (resolver == null || reference == null)
		{
			EditorGUI.PropertyField(_Rect, _Property, _Label);
			return;
		}
		
		Component context = resolver.GetContext();
		
		if (context == null)
		{
			EditorGUI.PropertyField(_Rect, _Property, _Label);
			return;
		}
		
		EditorGUI.BeginChangeCheck();
		
		Object data = EditorGUI.ObjectField(
			new Rect(
				_Rect.x,
				_Rect.y + (_Rect.height - EditorGUIUtility.singleLineHeight) * 0.5f,
				_Rect.width,
				EditorGUIUtility.singleLineHeight
			),
			_Label,
			resolver.GetReference(reference.Type, _Property.stringValue),
			reference.Type,
			true
		);
		
		if (EditorGUI.EndChangeCheck())
		{
			_Property.stringValue = CreateReference(context, data);
			_Property.serializedObject.ApplyModifiedProperties();
		}
	}

	static string CreateReference(Component _Context, Object _Data)
	{
		if (_Context == null || _Data == null)
			return null;
		
		Transform transform;
		switch (_Data)
		{
			case GameObject gameObject:
				transform = gameObject.transform;
				break;
			case Component component:
				transform = component.transform;
				break;
			default:
				return null;
		}
		
		List<string> path = new List<string>();
		while (transform != null && transform != _Context.transform)
		{
			path.Add(transform.name);
			transform = transform.parent;
		}
		path.Reverse();
		
		return string.Join("/", path.ToArray());
	}
}