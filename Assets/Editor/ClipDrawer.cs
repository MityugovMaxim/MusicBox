using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
		
		Assembly assembly = typeof(ClipDrawer).Assembly;
		
		IEnumerable<Type> clipDrawerTypes = assembly.GetTypes().Where(_Type => _Type.IsSubclassOf(typeof(ClipDrawer)));
		
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

	public void Draw(Rect _Rect, Rect _R)
	{
		DrawBackground(_Rect, _R);
		DrawContent(_Rect, _R);
		DrawHandles(_Rect, _R);
	}

	protected virtual void DrawBackground(Rect _Rect, Rect _ViewRect)
	{
		EditorGUI.DrawRect(_Rect, Color.black);
	}

	protected virtual void DrawContent(Rect _Rect, Rect _R)
	{
		GUI.Label(_Rect, Clip.GetType().Name, EditorStyles.whiteLabel);
	}

	protected virtual void DrawHandles(Rect _Rect, Rect _R)
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