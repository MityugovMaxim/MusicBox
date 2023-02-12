using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(UISpline))]
public class UISplineEditor : Editor
{
	#region constants

	const string SPLINE_NORMALS_ENABLED_KEY = "SPLINE_NORMALS_ENABLED";
	const string SPLINE_NORMALS_LENGTH_KEY  = "SPLINE_NORMALS_LENGTH";

	#endregion

	UISpline Spline
	{
		get
		{
			if (m_Spline == null)
				m_Spline = target as UISpline;
			return m_Spline;
		}
	}

	Rect Rect => Spline.RectTransform.rect;

	#region attributes

	static bool  m_NormalsEnabled = true;
	static float m_NormalsLength  = 15;

	readonly List<int> m_SelectedKeys       = new List<int>();
	readonly List<int> m_SelectedKeysBuffer = new List<int>();

	UISpline m_Spline;

	bool    m_MoveInput;
	bool    m_AddInput;
	bool    m_InsertInput;
	bool    m_SelectInput;
	Vector2 m_SelectKeySource;
	Vector2 m_SelectKeyTarget;

	ReorderableList m_KeysList;

	#endregion

	#region engine methods

	void OnEnable()
	{
		Tools.hidden = true;
		
		CreateKeysList();
		
		m_NormalsEnabled = EditorPrefs.GetBool(SPLINE_NORMALS_ENABLED_KEY, m_NormalsEnabled);
		m_NormalsLength  = EditorPrefs.GetFloat(SPLINE_NORMALS_LENGTH_KEY, m_NormalsLength);
		
		Undo.undoRedoPerformed += OnPerformUndoRedo;
	}

	void OnDisable()
	{
		Tools.hidden = false;
		
		Undo.undoRedoPerformed -= OnPerformUndoRedo;
	}

	void OnPerformUndoRedo()
	{
		m_MoveInput       = false;
		m_SelectInput     = false;
		m_AddInput        = false;
		m_InsertInput     = false;
		m_SelectKeySource = Vector2.zero;
		m_SelectKeyTarget = Vector2.zero;
		m_SelectedKeys.Clear();
		m_SelectedKeysBuffer.Clear();
		
		SceneView.RepaintAll();
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		DrawParameters();
		
		DrawKeys();
		
		DrawNormals();
		
		serializedObject.ApplyModifiedProperties();
	}

	void OnSceneGUI()
	{
		UISpline spline = target as UISpline;
		
		if (spline == null)
			return;
		
		Matrix4x4 matrix = Handles.matrix;
		
		Handles.matrix = spline.RectTransform.localToWorldMatrix;
		
		DrawSceneSpline();
		
		DrawScenePoints();
		
		DrawSceneKeys();
		
		DrawSelectedKeysScene();
		
		if ((Event.current.modifiers & EventModifiers.Control) == 0)
		{
			Tools.hidden = true;
			
			SelectKeyInput();
			
			MoveKeysInput();
			
			InsertKeyInput();
			
			AddKeyInput();
			
			DeleteKeysInput();
			
			SelectKeysInput();
		}
		else
		{
			Tools.hidden = false;
		}
		
		serializedObject.ApplyModifiedProperties();
		
		Handles.matrix = matrix;
	}

	#endregion

	#region service methods

	void DrawParameters()
	{
		SerializedProperty samplesProperty = serializedObject.FindProperty("m_Samples");
		EditorGUILayout.PropertyField(samplesProperty);
		samplesProperty.intValue = Mathf.Max(samplesProperty.intValue, 1);
		
		SerializedProperty loopProperty = serializedObject.FindProperty("m_Loop");
		EditorGUILayout.PropertyField(loopProperty);
		
		SerializedProperty uniformProperty = serializedObject.FindProperty("m_Uniform");
		EditorGUILayout.PropertyField(uniformProperty);
		
		SerializedProperty optimizeProperty = serializedObject.FindProperty("m_Optimize");
		EditorGUILayout.PropertyField(optimizeProperty);
		
		if (optimizeProperty.boolValue)
		{
			SerializedProperty thresholdProperty = serializedObject.FindProperty("m_Threshold");
			thresholdProperty.floatValue = Mathf.Max(0, thresholdProperty.floatValue);
			EditorGUILayout.PropertyField(thresholdProperty);
		}
	}

	void DrawKeys()
	{
		if (m_KeysList != null)
			m_KeysList.DoLayoutList();
	}

	void DrawNormals()
	{
		EditorGUILayout.BeginHorizontal();
		
		GUI.backgroundColor = m_NormalsEnabled ? Color.white : Color.grey;
		if (GUILayout.Button("Normals", EditorStyles.miniButton, GUILayout.Width(80)))
		{
			m_NormalsEnabled = !m_NormalsEnabled;
			EditorPrefs.SetBool(SPLINE_NORMALS_ENABLED_KEY, m_NormalsEnabled);
			SceneView.RepaintAll();
		}
		GUI.backgroundColor = Color.white;
		
		EditorGUI.BeginChangeCheck();
		
		EditorGUIUtility.labelWidth = 70;
		
		m_NormalsLength = EditorGUILayout.FloatField("Length", m_NormalsLength);
		
		EditorGUIUtility.labelWidth = 0;
		
		if (EditorGUI.EndChangeCheck())
		{
			EditorPrefs.SetFloat(SPLINE_NORMALS_LENGTH_KEY, m_NormalsLength);
			SceneView.RepaintAll();
		}
		
		EditorGUILayout.EndHorizontal();
	}

	void CreateKeysList()
	{
		if (m_KeysList != null)
			return;
		
		SerializedProperty keysProperty = serializedObject.FindProperty("m_Keys");
		
		m_KeysList = new ReorderableList(serializedObject, keysProperty, true, true, true, true);
		
		m_KeysList.elementHeight = 50;
		
		m_KeysList.drawHeaderCallback += _Rect =>
		{
			EditorGUI.LabelField(_Rect, "Keys", EditorStyles.boldLabel);
		};
		
		m_KeysList.onSelectCallback += _List =>
		{
			int index = _List.index;
			
			m_SelectedKeys.Clear();
			m_SelectedKeys.Add(index);
			
			SceneView.RepaintAll();
		};
		
		m_KeysList.drawElementBackgroundCallback += (_Rect, _Index, _Active, _Focused) =>
		{
			if (_Active || _Focused)
				EditorGUI.DrawRect(_Rect, new Color(0.2f, 0.3f, 0.45f));
			else if (_Index % 2 == 0)
				EditorGUI.DrawRect(_Rect, new Color(0.25f, 0.25f, 0.25f));
			else
				EditorGUI.DrawRect(_Rect, new Color(0.3f, 0.3f, 0.3f));
		};
		
		m_KeysList.drawElementCallback += (_Rect, _Index, _, _) =>
		{
			SerializedProperty keyProperty        = m_KeysList.serializedProperty.GetArrayElementAtIndex(_Index);
			SerializedProperty positionProperty   = keyProperty.FindPropertyRelative("m_Position");
			SerializedProperty anchorProperty     = keyProperty.FindPropertyRelative("m_Anchor");
			SerializedProperty inTangentProperty  = keyProperty.FindPropertyRelative("m_InTangent");
			SerializedProperty outTangentProperty = keyProperty.FindPropertyRelative("m_OutTangent");
			
			float step = _Rect.height / 3;
			
			Rect positionRect = new Rect(
				_Rect.x,
				_Rect.y + step * 0,
				_Rect.width - 90,
				step
			);
			
			Rect inTangentRect = new Rect(
				_Rect.x,
				_Rect.y + step * 1,
				_Rect.width - 90,
				step
			);
			
			Rect outTangentRect = new Rect(
				_Rect.x,
				_Rect.y + step * 2,
				_Rect.width - 90,
				step
			);
			
			Rect anchorRect = new Rect(
				_Rect.x + _Rect.width - 90,
				_Rect.y,
				90,
				_Rect.height
			);
			
			positionProperty.vector2Value   = EditorGUI.Vector2Field(positionRect, "Position", positionProperty.vector2Value);
			inTangentProperty.vector2Value  = EditorGUI.Vector2Field(inTangentRect, "In", inTangentProperty.vector2Value);
			outTangentProperty.vector2Value = EditorGUI.Vector2Field(outTangentRect, "Out", outTangentProperty.vector2Value);
			anchorProperty.vector2Value     = EditorGUI.Vector2Field(anchorRect, GUIContent.none, anchorProperty.vector2Value);
		};
	}

	Vector2 GetMouseDelta()
	{
		Vector2 delta = Event.current.delta;
		
		UISpline spline = target as UISpline;
		
		if (spline == null)
			return delta;
		
		Matrix4x4 matrix = spline.RectTransform.worldToLocalMatrix;
		
		Vector2 sourcePosition = Event.current.mousePosition - delta;
		Vector2 targetPosition = Event.current.mousePosition;
		
		Ray sourceRay = HandleUtility.GUIPointToWorldRay(sourcePosition);
		Ray targetRay = HandleUtility.GUIPointToWorldRay(targetPosition);
		
		Vector3 sourcePoint = matrix.MultiplyPoint3x4(sourceRay.origin);
		Vector3 targetPoint = matrix.MultiplyPoint3x4(targetRay.origin);
		
		return targetPoint - sourcePoint;
	}

	Vector2 GetMousePosition()
	{
		Vector2 position = Event.current.mousePosition;
		
		UISpline spline = target as UISpline;
		
		if (spline == null)
			return position;
		
		Ray ray = HandleUtility.GUIPointToWorldRay(position);
		
		return spline.RectTransform.worldToLocalMatrix.MultiplyPoint3x4(ray.origin);
	}

	bool FindCurve(Vector2 _Position, float _Tolerance, out int _SourceKey, out int _TargetKey)
	{
		_SourceKey = -1;
		_TargetKey = -1;
		
		SerializedProperty keysProperty = serializedObject.FindProperty("m_Keys");
		SerializedProperty loopProperty = serializedObject.FindProperty("m_Loop");
		
		int length = loopProperty.boolValue
			? keysProperty.arraySize
			: keysProperty.arraySize - 1;
		
		float tolerance = HandleUtility.GetHandleSize(_Position) * _Tolerance;
		
		float minDistance = float.MaxValue;
		
		for (int i = 0; i < length; i++)
		{
			int sourceIndex = i;
			int targetIndex = (i + 1) % keysProperty.arraySize;
			
			SerializedProperty sourceKeyProperty = keysProperty.GetArrayElementAtIndex(sourceIndex);
			SerializedProperty targetKeyProperty = keysProperty.GetArrayElementAtIndex(targetIndex);
			
			SerializedProperty sourcePositionProperty = sourceKeyProperty.FindPropertyRelative("m_Position");
			SerializedProperty sourceAnchorProperty   = sourceKeyProperty.FindPropertyRelative("m_Anchor");
			SerializedProperty sourceTangentProperty  = sourceKeyProperty.FindPropertyRelative("m_OutTangent");
			
			SerializedProperty targetPositionProperty = targetKeyProperty.FindPropertyRelative("m_Position");
			SerializedProperty targetAnchorProperty   = targetKeyProperty.FindPropertyRelative("m_Anchor");
			SerializedProperty targetTangentProperty  = targetKeyProperty.FindPropertyRelative("m_InTangent");
			
			Vector2 sourcePosition = GetKeyPosition(
				sourcePositionProperty.vector2Value,
				sourceAnchorProperty.vector2Value
			);
			
			Vector2 targetPosition = GetKeyPosition(
				targetPositionProperty.vector2Value,
				targetAnchorProperty.vector2Value
			);
			
			float distance = HandleUtility.DistancePointBezier(
				_Position,
				sourcePosition,
				targetPosition,
				sourcePosition + sourceTangentProperty.vector2Value,
				targetPosition + targetTangentProperty.vector2Value
			);
			
			if (distance < tolerance && distance < minDistance)
			{
				_SourceKey  = sourceIndex;
				_TargetKey  = targetIndex;
				minDistance = distance;
			}
		}
		return _SourceKey >= 0 && _TargetKey >= 0;
	}

	int FindKey(Vector2 _Position, float _Tolerance)
	{
		SerializedProperty keysProperty = serializedObject.FindProperty("m_Keys");
		
		float tolerance = HandleUtility.GetHandleSize(_Position) * _Tolerance;
		
		float minDistance = float.MaxValue;
		
		int index = -1;
		
		for (int i = 0; i < keysProperty.arraySize; i++)
		{
			SerializedProperty keyProperty      = keysProperty.GetArrayElementAtIndex(i);
			SerializedProperty positionProperty = keyProperty.FindPropertyRelative("m_Position");
			
			Vector2 position = positionProperty.vector2Value;
			
			float distance = Vector2.Distance(position, _Position);
			
			if (distance < tolerance && distance < minDistance)
			{
				index       = i;
				minDistance = distance;
			}
		}
		
		return index;
	}

	int[] FindKeys(Rect _Rect)
	{
		SerializedProperty keysProperty = serializedObject.FindProperty("m_Keys");
		
		List<int> indices = new List<int>();
		for (int i = 0; i < keysProperty.arraySize; i++)
		{
			SerializedProperty keyProperty      = keysProperty.GetArrayElementAtIndex(i);
			SerializedProperty positionProperty = keyProperty.FindPropertyRelative("m_Position");
			
			Vector2 position = positionProperty.vector2Value;
			
			if (_Rect.Contains(position, true))
				indices.Add(i);
		}
		
		return indices.ToArray();
	}

	Vector2 GetKeyPosition(Vector2 _Position, Vector2 _Anchor)
	{
		return Vector2.Scale(Rect.size, _Anchor) + _Position;
	}

	void DrawSceneSpline()
	{
		SerializedProperty keysProperty = serializedObject.FindProperty("m_Keys");
		SerializedProperty loopProperty = serializedObject.FindProperty("m_Loop");
		
		int length = loopProperty.boolValue
			? keysProperty.arraySize
			: keysProperty.arraySize - 1;
		for (int i = 0; i < length; i++)
		{
			SerializedProperty sourceKeyProperty = keysProperty.GetArrayElementAtIndex(i);
			SerializedProperty targetKeyProperty = keysProperty.GetArrayElementAtIndex((i + 1) % keysProperty.arraySize);
			
			SerializedProperty sourcePositionProperty = sourceKeyProperty.FindPropertyRelative("m_Position");
			SerializedProperty sourceAnchorProperty   = sourceKeyProperty.FindPropertyRelative("m_Anchor");
			SerializedProperty sourceTangentProperty  = sourceKeyProperty.FindPropertyRelative("m_OutTangent");
			
			SerializedProperty targetPositionProperty = targetKeyProperty.FindPropertyRelative("m_Position");
			SerializedProperty targetAnchorProperty   = targetKeyProperty.FindPropertyRelative("m_Anchor");
			SerializedProperty targetTangentProperty  = targetKeyProperty.FindPropertyRelative("m_InTangent");
			
			Vector2 sourcePosition = GetKeyPosition(
				sourcePositionProperty.vector2Value,
				sourceAnchorProperty.vector2Value
			);
			
			Vector2 targetPosition = GetKeyPosition(
				targetPositionProperty.vector2Value,
				targetAnchorProperty.vector2Value
			);
			
			Handles.DrawBezier(
				sourcePosition,
				targetPosition,
				sourcePosition + sourceTangentProperty.vector2Value,
				targetPosition + targetTangentProperty.vector2Value,
				Color.white,
				null,
				2
			);
		}
	}

	void DrawScenePoints()
	{
		const float handleSize = 0.025f;
		
		UISpline spline = target as UISpline;
		
		if (spline == null)
			return;
		
		float step = 1.0f / (spline.Length - 1) * 0.5f;
		for (int i = 0; i < spline.Length; i++)
		{
			UISpline.Point point = spline[i];
			
			Handles.color = Color.HSVToRGB(step * i + 0.25f, 1, 1);
			
			if (m_NormalsEnabled)
			{
				Handles.DrawLine(
					point.Position,
					point.Position + point.Normal * m_NormalsLength
				);
			}
			
			Handles.DrawSolidDisc(
				point.Position,
				spline.RectTransform.forward,
				HandleUtility.GetHandleSize(point.Position) * handleSize
			);
			
			Handles.color = Color.white;
		}
	}

	void DrawSceneKeys()
	{
		if (Event.current.type != EventType.Repaint)
			return;
		
		SerializedProperty keysProperty = serializedObject.FindProperty("m_Keys");
		for (int i = 0; i < keysProperty.arraySize; i++)
		{
			SerializedProperty keyProperty      = keysProperty.GetArrayElementAtIndex(i);
			SerializedProperty positionProperty = keyProperty.FindPropertyRelative("m_Position");
			SerializedProperty anchorProperty   = keyProperty.FindPropertyRelative("m_Anchor");
			
			Vector2 position = GetKeyPosition(
				positionProperty.vector2Value,
				anchorProperty.vector2Value
			);
			
			Handles.color = new Color(0.35f, 0.6f, 0.85f);
			
			Handles.DrawSolidDisc(
				position,
				Vector3.forward,
				HandleUtility.GetHandleSize(position) * 0.06f
			);
			
			Handles.color = Color.white;
			
			Handles.DrawSolidDisc(
				position,
				Vector3.forward,
				HandleUtility.GetHandleSize(position) * 0.04f
			);
		}
	}

	void DrawSelectedKeysScene()
	{
		if (Event.current.type != EventType.Repaint)
			return;
		
		SerializedProperty keysProperty = serializedObject.FindProperty("m_Keys");
		
		Handles.color = new Color(0.35f, 0.6f, 0.85f);
		foreach (int index in m_SelectedKeys)
		{
			if (index >= keysProperty.arraySize)
				continue;
			
			SerializedProperty keyProperty        = keysProperty.GetArrayElementAtIndex(index);
			SerializedProperty positionProperty   = keyProperty.FindPropertyRelative("m_Position");
			SerializedProperty anchorProperty     = keyProperty.FindPropertyRelative("m_Anchor");
			SerializedProperty inTangentProperty  = keyProperty.FindPropertyRelative("m_InTangent");
			SerializedProperty outTangentProperty = keyProperty.FindPropertyRelative("m_OutTangent");
			
			Vector2 position = GetKeyPosition(
				positionProperty.vector2Value,
				anchorProperty.vector2Value
			);
			
			Handles.DrawSolidDisc(
				position,
				Vector3.forward,
				HandleUtility.GetHandleSize(position) * 0.06f
			);
			
			DrawSceneTangent(
				positionProperty,
				inTangentProperty,
				outTangentProperty
			);
			
			DrawSceneTangent(
				positionProperty,
				outTangentProperty,
				inTangentProperty
			);
		}
		Handles.color = Color.white;
	}

	void DrawSceneTangent(SerializedProperty _PositionProperty, SerializedProperty _SourceTangent, SerializedProperty _TargetTangent)
	{
		const float handleSize = 0.035f;
		
		Vector2 position       = _PositionProperty.vector2Value;
		Vector2 sourcePosition = position + _SourceTangent.vector2Value;
		Vector2 targetPosition = position + _SourceTangent.vector2Value;
		
		Handles.DrawLine(position, sourcePosition);
		
		targetPosition = Handles.FreeMoveHandle(
			targetPosition,
			Quaternion.identity,
			HandleUtility.GetHandleSize(targetPosition) * handleSize,
			Vector3.zero,
			DiscHandleCap
		);
		
		if (sourcePosition == targetPosition)
			return;
		
		sourcePosition -= position;
		targetPosition -= position;
		
		_SourceTangent.vector2Value = targetPosition;
		
		if (Event.current.alt)
			return;
		
		Vector2 tangent = _TargetTangent.vector2Value;
		
		float angle = Vector2.SignedAngle(sourcePosition, targetPosition);
		
		tangent = tangent.normalized * (tangent.magnitude + targetPosition.magnitude - sourcePosition.magnitude);
		
		_TargetTangent.vector2Value = Quaternion.Euler(0, 0, angle) * tangent;
	}

	void SelectKeyInput()
	{
		switch (Event.current.type)
		{
			case EventType.Layout:
			{
				int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
				
				HandleUtility.AddDefaultControl(controlID);
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (Event.current.button != 0)
					break;
				
				if (Event.current.modifiers != EventModifiers.None && Event.current.modifiers != EventModifiers.Command)
					break;
				
				Vector2 position = GetMousePosition();
				
				int key = FindKey(position, 0.08f);
				
				if (key < 0)
					break;
				
				if (Event.current.modifiers == EventModifiers.Command)
				{
					if (m_SelectedKeys.Contains(key))
					{
						m_SelectedKeys.Remove(key);
						Event.current.Use();
					}
					else
					{
						m_SelectedKeys.Add(key);
					}
				}
				else if (!m_SelectedKeys.Contains(key))
				{
					m_SelectedKeys.Clear();
					m_SelectedKeys.Add(key);
				}
				
				SceneView.RepaintAll();
				
				break;
			}
		}
	}

	void MoveKeysInput()
	{
		switch (Event.current.type)
		{
			case EventType.Layout:
			{
				int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
				
				HandleUtility.AddDefaultControl(controlID);
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (Event.current.button != 0)
					break;
				
				if (Event.current.modifiers != EventModifiers.None && Event.current.modifiers != EventModifiers.Command)
					break;
				
				Vector2 position = GetMousePosition();
				
				int key = FindKey(position, 0.08f);
				
				if (key < 0)
					break;
				
				if (!m_SelectedKeys.Contains(key))
					break;
				
				m_MoveInput = true;
				
				Event.current.Use();
				
				break;
			}
			
			case EventType.MouseDrag:
			{
				if (!m_MoveInput)
					break;
				
				Vector2 delta = GetMouseDelta();
				
				SerializedProperty keysProperty = serializedObject.FindProperty("m_Keys");
				foreach (int key in m_SelectedKeys)
				{
					SerializedProperty keyProperty      = keysProperty.GetArrayElementAtIndex(key);
					SerializedProperty positionProperty = keyProperty.FindPropertyRelative("m_Position");
					
					positionProperty.vector2Value += delta;
				}
				
				Event.current.Use();
				
				break;
			}
			
			case EventType.MouseUp:
			{
				if (!m_MoveInput)
					break;
				
				m_MoveInput = false;
				
				Event.current.Use();
				
				break;
			}
		}

		switch (Event.current.rawType)
		{
			case EventType.MouseUp:
			{
				m_MoveInput = false;
				
				break;
			}
		}
	}

	void SelectKeysInput()
	{
		switch (Event.current.type)
		{
			case EventType.Layout:
			{
				int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
				
				HandleUtility.AddDefaultControl(controlID);
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (Event.current.button != 0)
					break;
				
				if (Event.current.modifiers != EventModifiers.None && Event.current.modifiers != EventModifiers.Shift)
					break;
				
				Vector2 position = GetMousePosition();
				
				m_SelectInput     = true;
				m_SelectKeySource = position;
				m_SelectKeyTarget = position;
				
				m_SelectedKeysBuffer.Clear();
				
				if (Event.current.modifiers != EventModifiers.Shift)
					m_SelectedKeys.Clear();
				else
					m_SelectedKeysBuffer.AddRange(m_SelectedKeys);
				
				break;
			}
			
			case EventType.MouseDrag:
			{
				if (!m_SelectInput)
					break;
				
				m_SelectKeyTarget = GetMousePosition();
				
				Rect rect = Rect.MinMaxRect(
					m_SelectKeySource.x,
					m_SelectKeySource.y,
					m_SelectKeyTarget.x,
					m_SelectKeyTarget.y
				);
				
				m_SelectedKeys.Clear();
				
				m_SelectedKeys.AddRange(m_SelectedKeysBuffer);
				
				foreach (int key in FindKeys(rect))
				{
					if (!m_SelectedKeys.Contains(key))
						m_SelectedKeys.Add(key);
				}
				
				Event.current.Use();
				
				break;
			}
			
			case EventType.MouseUp:
			{
				if (!m_SelectInput)
					break;
				
				m_SelectInput     = false;
				m_SelectKeySource = Vector2.zero;
				m_SelectKeyTarget = Vector2.zero;
				
				m_SelectedKeysBuffer.Clear();
				
				SceneView.RepaintAll();
				
				break;
			}
			
			case EventType.Repaint:
			{
				if (!m_SelectInput)
					break;
				
				Rect rect = Rect.MinMaxRect(
					m_SelectKeySource.x,
					m_SelectKeySource.y,
					m_SelectKeyTarget.x,
					m_SelectKeyTarget.y
				);
				
				Handles.DrawSolidRectangleWithOutline(
					rect,
					Color.clear,
					Color.white
				);
				
				break;
			}
		}
		
		switch (Event.current.rawType)
		{
			case EventType.MouseUp:
			{
				m_SelectInput     = false;
				m_SelectKeySource = Vector2.zero;
				m_SelectKeyTarget = Vector2.zero;
				
				m_SelectedKeysBuffer.Clear();
				
				SceneView.RepaintAll();
				
				break;
			}
		}
	}

	void AddKeyInput()
	{
		switch (Event.current.type)
		{
			case EventType.Layout:
			{
				int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
				
				HandleUtility.AddDefaultControl(controlID);
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (Event.current.button != 0 || Event.current.modifiers != EventModifiers.Command)
					break;
				
				m_AddInput = true;
				
				Vector2 position = GetMousePosition();
				
				SerializedProperty keysProperty = serializedObject.FindProperty("m_Keys");
				
				int key = keysProperty.arraySize;
				
				keysProperty.InsertArrayElementAtIndex(key);
				
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
				
				SerializedProperty keyProperty        = keysProperty.GetArrayElementAtIndex(key);
				SerializedProperty positionProperty   = keyProperty.FindPropertyRelative("m_Position");
				SerializedProperty anchorProperty     = keyProperty.FindPropertyRelative("m_Anchor");
				SerializedProperty inTangentProperty  = keyProperty.FindPropertyRelative("m_InTangent");
				SerializedProperty outTangentProperty = keyProperty.FindPropertyRelative("m_OutTangent");
				
				positionProperty.vector2Value   = position;
				anchorProperty.vector2Value     = Vector2.zero;
				inTangentProperty.vector2Value  = Vector2.zero;
				outTangentProperty.vector2Value = Vector2.zero;
				
				m_SelectedKeys.Clear();
				m_SelectedKeys.Add(key);
				
				Event.current.Use();
				
				break;
			}
			
			case EventType.MouseDrag:
			{
				if (!m_AddInput)
					break;
				
				Vector2 position = GetMousePosition();
				
				int key = m_SelectedKeys.FirstOrDefault();
				
				SerializedProperty keysProperty       = serializedObject.FindProperty("m_Keys");
				SerializedProperty keyProperty        = keysProperty.GetArrayElementAtIndex(key);
				SerializedProperty positionProperty   = keyProperty.FindPropertyRelative("m_Position");
				SerializedProperty anchorProperty     = keyProperty.FindPropertyRelative("m_Anchor");
				SerializedProperty inTangentProperty  = keyProperty.FindPropertyRelative("m_InTangent");
				SerializedProperty outTangentProperty = keyProperty.FindPropertyRelative("m_OutTangent");
				
				Vector2 tangent = position - GetKeyPosition(positionProperty.vector2Value, anchorProperty.vector2Value);
				
				inTangentProperty.vector2Value  = -tangent;
				outTangentProperty.vector2Value = tangent;
				
				Event.current.Use();
				
				break;
			}
			
			case EventType.MouseUp:
			{
				if (!m_AddInput)
					break;
				
				m_AddInput = false;
				
				Event.current.Use();
				
				break;
			}
		}
		
		switch (Event.current.rawType)
		{
			case EventType.MouseUp:
			{
				m_AddInput = false;
				
				break;
			}
		}
	}

	void InsertKeyInput()
	{
		switch (Event.current.type)
		{
			case EventType.Layout:
			{
				int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
				
				HandleUtility.AddDefaultControl(controlID);
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (Event.current.button != 0 || Event.current.modifiers != EventModifiers.Command)
					break;
				
				Vector2 position = GetMousePosition();
				
				if (!FindCurve(position, 0.08f, out _, out int targetKey))
					break;
				
				m_InsertInput = true;
				
				SerializedProperty keysProperty = serializedObject.FindProperty("m_Keys");
				
				keysProperty.InsertArrayElementAtIndex(targetKey);
				
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
				
				SerializedProperty keyProperty        = keysProperty.GetArrayElementAtIndex(targetKey);
				SerializedProperty positionProperty   = keyProperty.FindPropertyRelative("m_Position");
				SerializedProperty anchorProperty     = keyProperty.FindPropertyRelative("m_Anchor");
				SerializedProperty inTangentProperty  = keyProperty.FindPropertyRelative("m_InTangent");
				SerializedProperty outTangentProperty = keyProperty.FindPropertyRelative("m_OutTangent");
				
				positionProperty.vector2Value   = position;
				anchorProperty.vector2Value     = Vector2.zero;
				inTangentProperty.vector2Value  = Vector2.zero;
				outTangentProperty.vector2Value = Vector2.zero;
				
				m_SelectedKeys.Clear();
				m_SelectedKeys.Add(targetKey);
				
				Event.current.Use();
				
				break;
			}
			
			case EventType.MouseDrag:
			{
				if (!m_InsertInput)
					break;
				
				Vector2 position = GetMousePosition();
				
				int key = m_SelectedKeys.FirstOrDefault();
				
				SerializedProperty keysProperty       = serializedObject.FindProperty("m_Keys");
				SerializedProperty keyProperty        = keysProperty.GetArrayElementAtIndex(key);
				SerializedProperty positionProperty   = keyProperty.FindPropertyRelative("m_Position");
				SerializedProperty anchorProperty     = keyProperty.FindPropertyRelative("m_Anchor");
				SerializedProperty inTangentProperty  = keyProperty.FindPropertyRelative("m_InTangent");
				SerializedProperty outTangentProperty = keyProperty.FindPropertyRelative("m_OutTangent");
				
				Vector2 tangent = position - GetKeyPosition(positionProperty.vector2Value, anchorProperty.vector2Value);
				
				inTangentProperty.vector2Value  = -tangent;
				outTangentProperty.vector2Value = tangent;
				
				Event.current.Use();
				
				break;
			}
			
			case EventType.MouseUp:
			{
				if (!m_InsertInput)
					break;
				
				m_InsertInput = false;
				
				Event.current.Use();
				
				break;
			}
		}
		
		switch (Event.current.rawType)
		{
			case EventType.MouseUp:
			{
				m_InsertInput = false;
				
				break;
			}
		}
	}

	void DeleteKeysInput()
	{
		switch (Event.current.type)
		{
			case EventType.KeyDown:
			{
				if (Event.current.keyCode != KeyCode.Backspace && Event.current.keyCode != KeyCode.Delete)
					break;
				
				if (m_SelectedKeys.Count == 0)
					break;
				
				Event.current.Use();
				
				SerializedProperty keysProperty = serializedObject.FindProperty("m_Keys");
				
				m_SelectedKeys.Reverse();
				foreach (int key in m_SelectedKeys)
					keysProperty.DeleteArrayElementAtIndex(key);
				m_SelectedKeys.Clear();
				
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
				
				break;
			}
		}
	}

	static void DiscHandleCap(
		int        _ControlID,
		Vector3    _Position,
		Quaternion _Rotation,
		float      _Size,
		EventType  _EventType
	)
	{
		if (_EventType != EventType.Layout)
		{
			if (_EventType != EventType.Repaint)
				return;
			
			Vector3 normal = _Rotation * new Vector3(0.0f, 0.0f, 1f);
			
			Handles.DrawSolidDisc(_Position, normal, _Size);
		}
		else
		{
			HandleUtility.AddControl(
				_ControlID,
				HandleUtility.DistanceToRectangle(_Position, _Rotation, _Size)
			);
		}
	}

	#endregion
}
