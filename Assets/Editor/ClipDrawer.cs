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

	public void Draw(Rect _Rect)
	{
		DrawBackground(_Rect);
		DrawContent(_Rect);
		DrawHandles(_Rect);
	}

	public virtual void DrawBackground(Rect _Rect)
	{
		EditorGUI.DrawRect(_Rect, Color.black);
	}

	public virtual void DrawContent(Rect _Rect)
	{
		GUI.Label(_Rect, Clip.GetType().Name, EditorStyles.whiteLabel);
	}

	public virtual void DrawHandles(Rect _Rect)
	{
		EditorGUI.DrawRect(
			new Rect(
				_Rect.xMin,
				_Rect.y,
				4,
				_Rect.height
			),
			Color.white
		);
		
		EditorGUI.DrawRect(
			new Rect(
				_Rect.xMax - 4,
				_Rect.y,
				4,
				_Rect.height
			),
			Color.white
		);
	}
}