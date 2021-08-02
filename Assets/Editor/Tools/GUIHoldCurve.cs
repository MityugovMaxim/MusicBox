using UnityEditor;
using UnityEngine;

public static class GUIHoldCurve
{
	const float SNAP = 1.0f / 6.0f;

	static int m_ControlID;

	static HoldCurve.Key m_Selection;

	public static void DrawSpline(Rect _Rect, HoldCurve _HoldCurve, bool _Interactable = true)
	{
		m_ControlID = $"spline_curve_control_id_${_HoldCurve.GetHashCode()}".GetHashCode();
		
		DrawBackground(_Rect);
		
		DrawCurve(_Rect, _HoldCurve);
		
		DrawKeys(_Rect, _HoldCurve);
		
		if (_Interactable)
		{
			SelectInput(_Rect, _HoldCurve);
			
			AddInput(_Rect, _HoldCurve);
			
			RemoveInput(_HoldCurve);
			
			MoveInput(_Rect, _HoldCurve);
		}
	}

	static void DrawBackground(Rect _Rect)
	{
		EditorGUI.DrawRect(_Rect, new Color(0.12f, 0.12f, 0.12f));
		
		if (Mathf.Approximately(SNAP, 0))
			return;
		
		int count = Mathf.CeilToInt(1.0f / SNAP);
		
		Handles.color = new Color(0.4f, 0.4f, 0.4f);
		for (int i = 1; i < count; i++)
		{
			float position = i * SNAP;
			
			Vector2 source = new Vector2(_Rect.xMin, _Rect.y + _Rect.height * position);
			Vector2 target = new Vector2(_Rect.xMax, _Rect.y + _Rect.height * position);
			
			Handles.DrawLine(source, target);
		}
		Handles.color = Color.white;
	}

	static void DrawCurve(Rect _Rect, HoldCurve _HoldCurve)
	{
		if (Event.current.type != EventType.Repaint)
			return;
		
		for (int i = 1; i < _HoldCurve.Length; i++)
		{
			HoldCurve.Key source = _HoldCurve[i - 1];
			HoldCurve.Key target = _HoldCurve[i];
			
			Vector2 sourcePosition = GetKeyPosition(_Rect, source);
			Vector2 targetPosition = GetKeyPosition(_Rect, target);
			
			if (sourcePosition == targetPosition)
				continue;
			
			Vector2 sourceTangent = Vector2.Scale(_Rect.size, source.OutTangent);
			Vector2 targetTangent = Vector2.Scale(_Rect.size, target.InTangent);
			
			Handles.DrawBezier(
				sourcePosition,
				targetPosition,
				sourcePosition + sourceTangent,
				targetPosition + targetTangent,
				GUIUtility.hotControl == m_ControlID && (source == m_Selection || target == m_Selection)
					? new Color(0.25f, 0.5f, 1f)
					: Color.white,
				null,
				2
			);
		}
	}

	static void DrawKeys(Rect _Rect, HoldCurve _HoldCurve)
	{
		if (Event.current.type != EventType.Repaint)
			return;
		
		foreach (var key in _HoldCurve)
		{
			Vector2 position = GetKeyPosition(_Rect, key);
			Handles.color = key == m_Selection ? new Color(0.25f, 0.5f, 1f) : Color.white;
			Handles.DrawSolidDisc(position, Vector3.back, 2);
			Handles.color = Color.white;
		}
	}

	static HoldCurve.Key FindKey(Vector2 _Position, Rect _Rect, HoldCurve _HoldCurve)
	{
		const float tolerance = 5 * 5;
		
		foreach (var key in _HoldCurve)
		{
			Vector2 position = new Vector2(
				_Rect.x + _Rect.width * key.Time,
				_Rect.y + _Rect.height * key.Value
			);
			
			if ((_Position - position).sqrMagnitude < tolerance)
				return key;
		}
		
		return null;
	}

	static Vector2 GetKeyPosition(Rect _Rect, HoldCurve.Key _Key)
	{
		return new Vector2(
			_Rect.x + _Rect.width * _Key.Time,
			_Rect.y + _Rect.height * _Key.Value
		);
	}

	static float GetKeyTime(Rect _Rect, Vector2 _Position)
	{
		return MathUtility.Remap01(_Position.x, _Rect.xMin, _Rect.xMax);
	}

	static float GetKeyValue(Rect _Rect, Vector2 _Position)
	{
		return MathUtility.Remap01(_Position.y, _Rect.yMin, _Rect.yMax);
	}

	static void SelectInput(Rect _Rect, HoldCurve _HoldCurve)
	{
		Vector2 position = Event.current.mousePosition;
		switch (Event.current.type)
		{
			case EventType.MouseDown:
			{
				if (!_Rect.Contains(position))
					break;
				
				if (Event.current.button != 0)
					break;
				
				GUI.FocusControl(null);
				
				var key = FindKey(position, _Rect, _HoldCurve);
				
				if (key == null)
				{
					m_Selection = null;
					break;
				}
				
				m_Selection = key;
				
				break;
			}
		}
	}

	static void AddInput(Rect _Rect, HoldCurve _HoldCurve)
	{
		Vector2 position = Event.current.mousePosition;
		switch (Event.current.type)
		{
			case EventType.MouseDown:
			{
				if (!_Rect.Contains(position))
					break;
				
				if (Event.current.button != 0)
					break;
				
				if (Event.current.modifiers != EventModifiers.Command)
					break;
				
				float time  = GetKeyTime(_Rect, position);
				float value = GetKeyValue(_Rect, position);
				
				var key = new HoldCurve.Key(
					Mathf.Clamp01(time),
					Mathf.Clamp01(value),
					Vector2.zero,
					Vector2.zero
				);
				
				_HoldCurve.Add(key);
				_HoldCurve.Reposition();
				
				m_Selection = key;
				
				break;
			}
		}
	}

	static void RemoveInput(HoldCurve _HoldCurve)
	{
		switch (Event.current.type)
		{
			case EventType.ValidateCommand:
			{
				if (Event.current.commandName != "Delete")
					break;
				
				if (m_Selection == null)
					break;
				
				Event.current.Use();
				
				break;
			}
			
			case EventType.ExecuteCommand:
			{
				if (Event.current.commandName != "Delete")
					break;
				
				if (m_Selection == null)
					break;
				
				Event.current.Use();
				
				_HoldCurve.Remove(m_Selection);
				_HoldCurve.Reposition();
				
				m_Selection = null;
				
				EditorGUIUtility.ExitGUI();
				
				break;
			}
		}
	}

	static void MoveInput(Rect _Rect, HoldCurve _HoldCurve)
	{
		Vector2 position = Event.current.mousePosition;
		
		switch (Event.current.type)
		{
			case EventType.MouseDown:
			{
				if (!_Rect.Contains(position))
					break;
				
				if (Event.current.button != 0)
					break;
				
				var key = FindKey(position, _Rect, _HoldCurve);
				
				if (key == null || key != m_Selection)
					break;
				
				Event.current.Use();
				
				GUI.FocusControl(null);
				GUIUtility.hotControl = m_ControlID;
				
				Event.current.SetPosition(GetKeyPosition(_Rect, key));
				
				key.Value = MathUtility.Snap(key.Value, SNAP);
				
				break;
			}
			
			case EventType.MouseDrag:
			{
				if (GUIUtility.hotControl != m_ControlID)
					break;
				
				Event.current.Use();
				
				if (m_Selection == null)
					break;
				
				Vector2 input = Event.current.GetPosition();
				
				float time  = GetKeyTime(_Rect, input);
				float value = GetKeyValue(_Rect, input);
				
				value = MathUtility.Snap(value, SNAP);
				
				m_Selection.Time  = Mathf.Clamp01(time);
				m_Selection.Value = Mathf.Clamp01(value);
				
				_HoldCurve.Reposition();
				
				break;
			}
			
			case EventType.MouseUp:
			{
				if (GUIUtility.hotControl != m_ControlID)
					break;
				
				Event.current.Use();
				
				GUIUtility.hotControl = 0;
				
				_HoldCurve.Reposition();
				
				break;
			}
		}
	}
}