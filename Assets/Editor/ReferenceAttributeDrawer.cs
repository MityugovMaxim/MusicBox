using System.Text;
using UnityEditor;
using UnityEngine;

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
		
		Component component = EditorGUI.ObjectField(
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
		) as Component;
		
		if (EditorGUI.EndChangeCheck())
			_Property.stringValue = CreateReference(context, component);
	}

	static string CreateReference(Component _Context, Component _Component)
	{
		if (_Context == null || _Component == null)
			return null;
		
		StringBuilder reference = new StringBuilder();
		
		Transform transform = _Component.transform;
		while (transform != null)
		{
			reference.Insert(0, transform.name);
			
			transform = transform.parent;
			
			if (transform == _Context.transform)
				break;
			
			reference.Insert(0, '/');
		}
		
		return reference.ToString();
	}
}