using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ClipDrawer
{
	static readonly Dictionary<Type, Type> m_ClipDrawerTypes = new Dictionary<Type, Type>();

	public void Setup(params Vector2Int[] _Value)
	{
		Clear();
	}

	public void Clear()
	{
		
	}

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

	protected Clip             Clip       { get; }
	protected SerializedObject ClipObject { get; }

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

	protected ClipDrawer(Clip _Clip)
	{
		Clip       = _Clip;
		ClipObject = new SerializedObject(Clip);
		
		int controlID = Clip.GetInstanceID();
		
		MinTimeProperty = ClipObject.FindProperty("m_MinTime");
		MaxTimeProperty = ClipObject.FindProperty("m_MaxTime");
		
		LeftHandleControlID   = EditorGUIUtility.GetControlID($"[{controlID}sequencer_left_handle_control]".GetHashCode(), FocusType.Passive);
		CenterHandleControlID = EditorGUIUtility.GetControlID($"[{controlID}sequencer_center_handle_control]".GetHashCode(), FocusType.Passive);
		RightHandleControlID  = EditorGUIUtility.GetControlID($"[{controlID}sequencer_right_handle_control]".GetHashCode(), FocusType.Passive);
	}

	public void Draw(Rect _TrackRect, float _TrackMinTime, float _TrackMaxTime)
	{
		TrackRect    = _TrackRect;
		TrackMinTime = _TrackMinTime;
		TrackMaxTime = _TrackMaxTime;
		
		float clipMin = MathUtility.Remap01(MinTime, TrackMinTime, TrackMaxTime);
		float clipMax = MathUtility.Remap01(MaxTime, TrackMinTime, TrackMaxTime);
		
		ClipRect = new Rect(
			_TrackRect.x + _TrackRect.width * clipMin,
			_TrackRect.y,
			_TrackRect.width * (clipMax - clipMin),
			_TrackRect.height
		);
		
		float viewMin = MathUtility.Remap01Clamped(MinTime, TrackMinTime, TrackMaxTime);
		float viewMax = MathUtility.Remap01Clamped(MaxTime, TrackMinTime, TrackMaxTime);
		
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
		DrawSelection();
		DrawHandles();
		
		DeleteInput();
	}

	protected virtual void DrawBackground()
	{
		EditorGUI.DrawRect(ClipRect, Color.black);
	}

	protected virtual void DrawContent()
	{
		GUI.Label(ViewRect, Clip.name, EditorStyles.whiteLabel);
	}

	protected virtual void DrawSelection()
	{
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				if (Selection.Contains(Clip))
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
					Selection.activeObject = Clip;
				
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
					SetMousePosition(ClipRect);
					
					GUIUtility.hotControl = LeftHandleControlID;
					
					Event.current.Use();
				}
				
				if (rightHandleRect.Contains(Event.current.mousePosition))
				{
					SetMousePosition(ClipRect);
					
					GUIUtility.hotControl = RightHandleControlID;
					
					Event.current.Use();
				}
				
				if (centerHandleRect.Contains(Event.current.mousePosition))
				{
					SetMousePosition(ClipRect);
					
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
		
		ClipObject.ApplyModifiedProperties();
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
				Track  track = AssetDatabase.LoadMainAssetAtPath(path) as Track;
				
				using (SerializedObject trackObject = new SerializedObject(track))
				{
					SerializedProperty clipsProperty = trackObject.FindProperty("m_Clips");
					
					for (int i = 0; i < clipsProperty.arraySize; i++)
					{
						SerializedProperty clipProperty = clipsProperty.GetArrayElementAtIndex(i);
						
						Clip clip = clipProperty.objectReferenceValue as Clip;
						
						if (Clip != clip)
							continue;
						
						clipProperty.objectReferenceValue = null;
						
						clipsProperty.DeleteArrayElementAtIndex(i);
					}
					
					trackObject.ApplyModifiedProperties();
				}
				
				AssetDatabase.RemoveObjectFromAsset(Clip);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				
				Event.current.Use();
				
				break;
			}
		}
	}
}