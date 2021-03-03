using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ClipDrawer
{
	static readonly Dictionary<string, Type> m_ClipDrawerTypes = new Dictionary<string, Type>();

	public static ClipDrawer Create(SerializedProperty _Property)
	{
		if (_Property == null)
			return null;
		
		Type clipDrawerType = GetClipDrawerType(_Property);
		
		return Activator.CreateInstance(clipDrawerType, _Property) as ClipDrawer;
	}

	static Type GetClipDrawerType(SerializedProperty _Property)
	{
		if (m_ClipDrawerTypes.ContainsKey(_Property.type) && m_ClipDrawerTypes[_Property.type] != null)
			return m_ClipDrawerTypes[_Property.type];
		
		Assembly assembly = typeof(ClipDrawer).Assembly;
		
		IEnumerable<Type> clipDrawerTypes = assembly.GetTypes().Where(_Type => _Type.IsSubclassOf(typeof(ClipDrawer)));
		
		Type clipType = typeof(Clip).Assembly.GetType(_Property.type, true, true);
		
		foreach (Type clipDrawerType in clipDrawerTypes)
		{
			ClipDrawerAttribute attribute = clipDrawerType.GetCustomAttribute<ClipDrawerAttribute>();
			
			if (attribute.ClipType == clipType)
			{
				m_ClipDrawerTypes[_Property.type] = clipDrawerType;
				
				return clipDrawerType;
			}
		}
		
		return typeof(ClipDrawer);
	}

	protected SerializedProperty Property { get; }

	protected virtual bool Visible => TrackMinTime < MaxTime && TrackMaxTime > MinTime;

	protected float MinTime
	{
		get => MinTimeProperty.floatValue;
		set => MinTimeProperty.floatValue = value;
	}

	protected float MaxTime
	{
		get => MaxTimeProperty.floatValue;
		set => MaxTimeProperty.floatValue = value;
	}

	protected Rect TrackRect { get; private set; }
	protected float TrackMinTime { get; private set; }
	protected float TrackMaxTime { get; private set; }
	protected Rect ClipRect  { get; private set; }
	protected Rect ViewRect  { get; private set; }

	protected int LeftHandleControlID   { get; }
	protected int CenterHandleControlID { get; }
	protected int RightHandleControlID  { get; }

	SerializedProperty MinTimeProperty { get; }

	SerializedProperty MaxTimeProperty { get; }

	Vector2 m_MouseOrigin;

	protected ClipDrawer(SerializedProperty _Property)
	{
		Property = _Property;
		
		int controlID = base.GetHashCode();
		
		MinTimeProperty  = Property.FindPropertyRelative("m_MinTime");
		MaxTimeProperty = Property.FindPropertyRelative("m_MaxTime");
		
		LeftHandleControlID   = $"[{controlID}sequencer_left_handle_control]".GetHashCode();
		CenterHandleControlID = $"[{controlID}sequencer_center_handle_control]".GetHashCode();
		RightHandleControlID  = $"[{controlID}sequencer_right_handle_control]".GetHashCode(); 
	}

	public void Draw(Rect _TrackRect, float _MinTime, float _MaxTime)
	{
		TrackRect = _TrackRect;
		TrackMinTime   = _MinTime;
		TrackMaxTime   = _MaxTime;
		
		float clipMin = MathUtility.Remap01(MinTime, _MinTime, _MaxTime);
		float clipMax = MathUtility.Remap01(MaxTime, _MinTime, _MaxTime);
		
		ClipRect = new Rect(
			_TrackRect.x + _TrackRect.width * clipMin,
			_TrackRect.y,
			_TrackRect.width * (clipMax - clipMin),
			_TrackRect.height
		);
		
		float viewMin = MathUtility.Remap01Clamped(MinTime, _MinTime, _MaxTime);
		float viewMax = MathUtility.Remap01Clamped(MaxTime, _MinTime, _MaxTime);
		
		ViewRect = new Rect(
			_TrackRect.x + _TrackRect.width * viewMin,
			_TrackRect.y,
			_TrackRect.width * (viewMax - viewMin),
			_TrackRect.height
		);
		
		if (Visible)
			Draw();
	}

	protected virtual void Draw()
	{
		DrawBackground();
		DrawContent();
		DrawHandles();
	}

	protected virtual void DrawBackground()
	{
		EditorGUI.DrawRect(ClipRect, Color.black);
	}

	protected virtual void DrawContent()
	{
		GUI.Label(ViewRect, Property.type, EditorStyles.whiteLabel);
	}

	protected virtual void DrawHandles()
	{
		const float minDuration = 0.1f;
		
		RectOffset handlePadding = new RectOffset(100, 100, 0, 0);
		
		Rect leftHandleRect = new Rect(
			ClipRect.xMin - 2,
			ClipRect.y + 1,
			4,
			ClipRect.height - 2
		);
		
		Rect centerHandleRect = new Rect(
			ClipRect.x + 4,
			ClipRect.y,
			ClipRect.width - 8,
			ClipRect.height
		);
		
		Rect rightHandleRect = new Rect(
			ClipRect.xMax - 2,
			ClipRect.y + 1,
			4,
			ClipRect.height - 2
		);
		
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				Handles.DrawSolidRectangleWithOutline(
					leftHandleRect,
					new Color(1, 1, 1, 0.5f),
					Color.black
				);
				
				Handles.DrawSolidRectangleWithOutline(
					rightHandleRect,
					new Color(1, 1, 1, 0.5f),
					Color.black
				);
				
				EditorGUIUtility.AddCursorRect(
					GUIUtility.hotControl == LeftHandleControlID
						? handlePadding.Add(leftHandleRect)
						: leftHandleRect,
					MouseCursor.SplitResizeLeftRight,
					LeftHandleControlID
				);
				
				EditorGUIUtility.AddCursorRect(
					GUIUtility.hotControl == CenterHandleControlID
						? handlePadding.Add(centerHandleRect)
						: centerHandleRect,
					MouseCursor.Pan,
					CenterHandleControlID
				);
				
				EditorGUIUtility.AddCursorRect(
					GUIUtility.hotControl == RightHandleControlID
						? new RectOffset(100, 100, 0, 0).Add(rightHandleRect)
						: rightHandleRect,
					MouseCursor.SplitResizeLeftRight,
					RightHandleControlID
				);
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (leftHandleRect.Contains(Event.current.mousePosition))
				{
					SetMousePosition(leftHandleRect);
					
					GUIUtility.hotControl = LeftHandleControlID;
					
					Event.current.Use();
				}
				
				if (rightHandleRect.Contains(Event.current.mousePosition))
				{
					SetMousePosition(rightHandleRect);
					
					GUIUtility.hotControl = RightHandleControlID;
					
					Event.current.Use();
				}
				
				if (centerHandleRect.Contains(Event.current.mousePosition))
				{
					SetMousePosition(centerHandleRect);
					
					GUIUtility.hotControl = CenterHandleControlID;
					
					Event.current.Use();
				}
				
				break;
			}
			
			case EventType.MouseDrag:
			{
				if (GUIUtility.hotControl == LeftHandleControlID)
				{
					float time = MathUtility.Remap(
						GetMousePosition().x,
						TrackRect.xMin,
						TrackRect.xMax,
						TrackMinTime,
						TrackMaxTime
					);
					
					if (Event.current.command)
						time = SnapTime(time);
					time = Mathf.Clamp(time, 0, MaxTime - minDuration);
					
					Resize(time, MaxTime);
					
					Event.current.Use();
				}
				
				if (GUIUtility.hotControl == RightHandleControlID)
				{
					float time = MathUtility.Remap(
						GetMousePosition().x,
						TrackRect.xMin,
						TrackRect.xMax,
						TrackMinTime,
						TrackMaxTime
					);
					
					if (Event.current.command)
						time = SnapTime(time);
					time = Mathf.Max(MinTime + minDuration, time);
					
					Resize(MinTime, time);
					
					Event.current.Use();
				}
				
				if (GUIUtility.hotControl == CenterHandleControlID)
				{
					float time = MathUtility.Remap(
						GetMousePosition().x,
						TrackRect.xMin,
						TrackRect.xMax,
						TrackMinTime,
						TrackMaxTime
					);
					
					if (Event.current.command)
						time = SnapTime(time);
					time = Mathf.Max(0, time);
					
					Resize(time, time + MaxTime - MinTime);
					
					Event.current.Use();
				}
				
				break;
			}
		}
	}

	protected void SetMousePosition(Rect _Rect)
	{
		m_MouseOrigin = _Rect.position - Event.current.mousePosition;
	}

	protected Vector2 GetMousePosition()
	{
		return m_MouseOrigin + Event.current.mousePosition;
	}

	protected float SnapTime(float _Time)
	{
		return MathUtility.Snap(_Time, TrackMinTime, TrackMaxTime, 0.01f, 0.1f, 1, 5);
	}

	protected virtual void Resize(float _MinTime, float _MaxTime)
	{
		MinTime  = _MinTime;
		MaxTime = _MaxTime;
		
		Property.serializedObject.ApplyModifiedProperties();
	}
}