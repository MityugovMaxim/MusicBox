using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ClipDrawer
{
	static readonly Dictionary<Type, Type> m_ClipDrawerTypes = new Dictionary<Type, Type>();

	public static ClipDrawer Create(Clip _Clip)
	{
		if (_Clip == null)
			return null;
		
		Type clipDrawerType = GetClipDrawerType(_Clip.GetType());
		
		return Activator.CreateInstance(clipDrawerType, _Clip) as ClipDrawer;
	}

	static Type GetClipDrawerType(Type _ClipType)
	{
		if (m_ClipDrawerTypes.ContainsKey(_ClipType) && m_ClipDrawerTypes[_ClipType] != null)
			return m_ClipDrawerTypes[_ClipType];
		
		Type[] clipDrawerTypes = typeof(ClipDrawer).GetNestedTypes();
		
		foreach (Type clipDrawerType in clipDrawerTypes)
		{
			ClipDrawerAttribute attribute = clipDrawerType.GetCustomAttribute<ClipDrawerAttribute>();
			
			if (attribute.ClipType == _ClipType)
			{
				m_ClipDrawerTypes[_ClipType] = clipDrawerType;
				
				return clipDrawerType;
			}
		}
		
		return typeof(ClipDrawer);
	}

	protected Clip Clip { get; }

	public ClipDrawer(Clip _Clip)
	{
		Clip = _Clip;
	}

	public virtual void Draw(Rect _Rect)
	{
		EditorGUI.DrawRect(_Rect, Color.black);
		
		EditorGUI.DrawRect(
			new Rect(
				_Rect.xMin - 2,
				_Rect.y,
				4,
				_Rect.height
			),
			Color.white
		);
		
		EditorGUI.DrawRect(
			new Rect(
				_Rect.xMax - 2,
				_Rect.y,
				4,
				_Rect.height
			),
			Color.white
		);
	}
}