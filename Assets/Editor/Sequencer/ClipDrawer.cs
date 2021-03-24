using System;
using System.Collections.Generic;
using System.Linq;
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
		
		Type clipType = _Clip.GetType();
		
		Type clipDrawerType = GetClipDrawerType(clipType);
		
		return Activator.CreateInstance(
			clipDrawerType, new object[] { _Clip }
		) as ClipDrawer;
	}

	static Type GetClipDrawerType(Type _ClipType)
	{
		if (m_ClipDrawerTypes.ContainsKey(_ClipType) && m_ClipDrawerTypes[_ClipType] != null)
			return m_ClipDrawerTypes[_ClipType];
		
		Assembly assembly = typeof(ClipDrawer).Assembly;
		
		IEnumerable<Type> clipDrawerTypes = assembly.GetTypes().Where(_Type => _Type.IsSubclassOf(typeof(ClipDrawer)));
		
		foreach (Type clipDrawerType in clipDrawerTypes)
		{
			SequencerDrawerAttribute attribute = clipDrawerType.GetCustomAttribute<SequencerDrawerAttribute>();
			
			if (attribute.Type == _ClipType)
			{
				m_ClipDrawerTypes[_ClipType] = clipDrawerType;
				
				return clipDrawerType;
			}
		}
		
		return typeof(ClipDrawer);
	}

	protected static GUIStyle ContentStyle
	{
		get
		{
			if (m_ContentStyle == null)
			{
				m_ContentStyle                  = new GUIStyle(GUI.skin.label);
				m_ContentStyle.alignment        = TextAnchor.UpperCenter;
				m_ContentStyle.contentOffset    = new Vector2(0, 5);
				m_ContentStyle.fontStyle        = FontStyle.Bold;
				m_ContentStyle.normal.textColor = Color.white;
			}
			return m_ContentStyle;
		}
	}

	protected Clip             Clip       { get; }
	protected SerializedObject ClipObject { get; }

	protected virtual bool Visible => TrackMinTime < Clip.MaxTime && TrackMaxTime > Clip.MinTime;

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

	static GUIStyle m_ContentStyle;

	public ClipDrawer(Clip _Clip)
	{
		Clip       = _Clip;
		ClipObject = new SerializedObject(Clip);
		
		int controlID = Clip.GetInstanceID();
		
		MinTimeProperty = ClipObject.FindProperty("m_MinTime");
		MaxTimeProperty = ClipObject.FindProperty("m_MaxTime");
		
		LeftHandleControlID   = EditorGUIUtility.GetControlID($"[{controlID}]clip_left_handle_contr`ol".GetHashCode(), FocusType.Passive);
		CenterHandleControlID = EditorGUIUtility.GetControlID($"[{controlID}]clip_center_handle_control".GetHashCode(), FocusType.Passive);
		RightHandleControlID  = EditorGUIUtility.GetControlID($"[{controlID}]clip_right_handle_control".GetHashCode(), FocusType.Passive);
	}

	public void Draw(Rect _TrackRect, float _TrackMinTime, float _TrackMaxTime)
	{
		TrackRect    = _TrackRect;
		TrackMinTime = _TrackMinTime;
		TrackMaxTime = _TrackMaxTime;
		
		float clipMin = MathUtility.Remap01(Clip.MinTime, TrackMinTime, TrackMaxTime);
		float clipMax = MathUtility.Remap01(Clip.MaxTime, TrackMinTime, TrackMaxTime);
		
		ClipRect = new Rect(
			_TrackRect.x + _TrackRect.width * clipMin,
			_TrackRect.y,
			_TrackRect.width * (clipMax - clipMin),
			_TrackRect.height
		);
		
		float viewMin = MathUtility.Remap01Clamped(Clip.MinTime, TrackMinTime, TrackMaxTime);
		float viewMax = MathUtility.Remap01Clamped(Clip.MaxTime, TrackMinTime, TrackMaxTime);
		
		ViewRect = new Rect(
			_TrackRect.x + _TrackRect.width * viewMin,
			_TrackRect.y,
			_TrackRect.width * (viewMax - viewMin),
			_TrackRect.height
		);
		
		if (Visible || Selection.Contains(Clip))
		{
			ClipObject.UpdateIfRequiredOrScript();
			
			Draw();
		}
	}

	protected virtual void Draw()
	{
		DrawBackground();
		DrawContent();
		DrawSelection();
		DrawHandles();
		
		DeleteInput();
	}

	protected virtual void DrawBackground()
	{
		EditorGUI.DrawRect(ClipRect, new Color(0.12f, 0.12f, 0.12f, 0.5f));
		
		AudioCurveRendering.DrawCurveFrame(ClipRect);
	}

	protected virtual void DrawContent()
	{
		EditorGUI.DropShadowLabel(ViewRect, Clip.name, ContentStyle);
	}

	protected virtual void DrawSelection()
	{
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				if (Selection.Contains(Clip) && Visible)
				{
					Handles.DrawSolidRectangleWithOutline(
						new RectOffset(1, 1, 1, 1).Remove(ViewRect),
						Color.clear,
						new Color(0.25f, 0.6f, 0.85f)
					);
				}
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (ViewRect.Contains(Event.current.mousePosition))
				{
					GUI.FocusControl(null);
					Selection.activeObject = Clip;
				}
				
				break;
			}
		}
	}

	protected virtual void DrawHandles()
	{
		const float minDuration = 0.1f;
		
		RectOffset handlePadding = new RectOffset(100, 100, 0, 0);
		
		Rect leftHandleRect = new Rect(
			ClipRect.xMin,
			ClipRect.y,
			8,
			ClipRect.height
		);
		
		Rect centerHandleRect = new Rect(
			ClipRect.x + 8,
			ClipRect.y,
			ClipRect.width - 16,
			ClipRect.height
		);
		
		Rect rightHandleRect = new Rect(
			ClipRect.xMax - 8,
			ClipRect.y,
			8,
			ClipRect.height
		);
		
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				EditorGUIUtility.AddCursorRect(
					GUIUtility.hotControl == LeftHandleControlID
						? handlePadding.Add(leftHandleRect)
						: leftHandleRect,
					MouseCursor.ResizeHorizontal,
					LeftHandleControlID
				);
				
				EditorGUIUtility.AddCursorRect(
					GUIUtility.hotControl == RightHandleControlID
						? handlePadding.Add(rightHandleRect)
						: rightHandleRect,
					MouseCursor.ResizeHorizontal,
					RightHandleControlID
				);
				
				EditorGUIUtility.AddCursorRect(
					GUIUtility.hotControl == CenterHandleControlID
						? handlePadding.Add(centerHandleRect)
						: centerHandleRect,
					MouseCursor.Pan,
					CenterHandleControlID
				);
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (leftHandleRect.Contains(Event.current.mousePosition))
				{
					Event.current.SetPosition(ClipRect.xMin);
					
					GUIUtility.hotControl = LeftHandleControlID;
					
					Event.current.Use();
				}
				
				if (rightHandleRect.Contains(Event.current.mousePosition))
				{
					Event.current.SetPosition(ClipRect.xMax);
					
					GUIUtility.hotControl = RightHandleControlID;
					
					Event.current.Use();
				}
				
				if (centerHandleRect.Contains(Event.current.mousePosition))
				{
					Event.current.SetPosition(ClipRect.xMin);
					
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
						Event.current.GetHorizontalPosition(),
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
						Event.current.GetHorizontalPosition(),
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
						Event.current.GetHorizontalPosition(),
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
			
			case EventType.MouseUp:
			{
				if (GUIUtility.hotControl != LeftHandleControlID && GUIUtility.hotControl != RightHandleControlID && GUIUtility.hotControl != CenterHandleControlID)
					break;
				
				GUIUtility.hotControl = 0;
				
				string path  = AssetDatabase.GetAssetPath(Clip);
				Track  track = AssetDatabase.LoadAssetAtPath<Track>(path);
				
				track.Sort();
				
				Event.current.Use();
				
				break;
			}
		}
	}

	protected float SnapTime(float _Time)
	{
		return MathUtility.Snap(_Time, TrackMinTime, TrackMaxTime, 0.01f, 0.1f, 1, 5);
	}

	protected virtual void Resize(float _MinTime, float _MaxTime)
	{
		MinTime  = _MinTime;
		MaxTime = _MaxTime;
		
		ClipObject.ApplyModifiedProperties();
	}

	protected void Reposition()
	{
		string path  = AssetDatabase.GetAssetPath(Clip);
		Track  track = AssetDatabase.LoadAssetAtPath<Track>(path);
		
		track.Sort();
	}

	void DeleteInput()
	{
		switch (Event.current.type)
		{
			case EventType.ValidateCommand:
			{
				if (Event.current.commandName == "Delete" && Selection.Contains(Clip))
					Event.current.Use();
				break;
			}
			
			case EventType.ExecuteCommand:
			{
				if (Event.current.commandName != "Delete" || !Selection.Contains(Clip))
					break;
				
				string path  = AssetDatabase.GetAssetPath(Clip);
				Track  track = AssetDatabase.LoadAssetAtPath<Track>(path);
				
				TrackUtility.RemoveClip(track, Clip);
				
				Event.current.Use();
				
				EditorGUIUtility.ExitGUI();
				
				break;
			}
		}
	}
}