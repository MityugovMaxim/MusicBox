using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class SequencerEditor : EditorWindow
{
	[MenuItem("Window/Sequencer")]
	public static void Open()
	{
		SequencerEditor window = GetWindow<SequencerEditor>();
		window.minSize = new Vector2(300, 300);
	}

	[DidReloadScripts]
	static void OnRecompile()
	{
		m_ClipDrawers.Clear();
	}

	static float TracksWidth
	{
		get => EditorPrefs.GetFloat("SEQUENCER_EDITOR_TRACKS_WIDTH", 120);
		set => EditorPrefs.SetFloat("SEQUENCER_EDITOR_TRACKS_WIDTH", Mathf.Max(120, value));
	}

	static readonly Dictionary<string, ClipDrawer>      m_ClipDrawers  = new Dictionary<string, ClipDrawer>();
	static readonly Dictionary<Track, SerializedObject> m_TrackObjects = new Dictionary<Track, SerializedObject>();

	[SerializeField] Sequencer m_Sequencer;
	[SerializeField] float     m_MinTime;
	[SerializeField] float     m_MaxTime;

	void OnEnable()
	{
		Undo.undoRedoPerformed += Repaint;
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
	}

	void OnPlayModeStateChanged(PlayModeStateChange _Obj)
	{
		Repaint();
	}

	void OnDisable()
	{
		Undo.undoRedoPerformed -= Repaint;
		EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
	}

	void OnInspectorUpdate()
	{
		Repaint();
	}

	void OnSelectionChange()
	{
		Sequencer sequencer = Selection.GetFiltered<Sequencer>(SelectionMode.Assets).FirstOrDefault();
		
		if (sequencer == null || sequencer == m_Sequencer)
			return;
		
		m_ClipDrawers.Clear();
		m_TrackObjects.Clear();
		
		m_Sequencer = sequencer;
		m_MinTime   = 0;
		m_MaxTime   = 60;
		
		foreach (Track track in m_Sequencer.Tracks)
		foreach (Clip clip in track)
			m_MaxTime = Mathf.Max(m_MaxTime, clip.MaxTime);
		
		Repaint();
	}

	void OnGUI()
	{
		if (m_Sequencer == null)
			return;
		
		Rect toolbarRect = new Rect(0, 0, TracksWidth, 25);
		
		Rect timelineRect = new Rect(TracksWidth, 0, position.width - TracksWidth, 25);
		
		Rect tracksRect = new Rect(0, 25, TracksWidth, position.height - 25);
		
		Rect clipsRect = new Rect(TracksWidth, 25, position.width - TracksWidth, position.height - 25);
		
		DrawTracks(tracksRect);
		DrawClips(clipsRect);
		DrawToolbar(toolbarRect);
		DrawTimeline(timelineRect);
		
		MoveInput(clipsRect);
		ZoomInput(clipsRect);
		ResizeTracksInput(tracksRect);
	}

	void DrawToolbar(Rect _Rect)
	{
		GUILayout.BeginArea(_Rect);
		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button(">"))
			m_Sequencer.Play();
		
		if (GUILayout.Button("||"))
			m_Sequencer.Pause();
		
		if (GUILayout.Button("[]"))
			m_Sequencer.Stop();
		
		GUILayout.EndHorizontal();
		
		GUILayout.EndArea();
	}

	void Update()
	{
		if (m_Sequencer != null && m_Sequencer.Playing)
			EditorApplication.QueuePlayerLoopUpdate();
		
		Repaint();
	}

	void DrawTimeline(Rect _Rect)
	{
		DrawTimelineGuide(_Rect, 0.01f);
		DrawTimelineGuide(_Rect, 0.1f, true);
		DrawTimelineGuide(_Rect, 1, true);
		DrawTimelineGuide(_Rect, 5, true);
		DrawTimelineGuide(_Rect, 20, true);
		DrawTimelineGuide(_Rect, 60, true);
		DrawTimelineSeeker(_Rect);
	}

	void DrawTimelineGuide(Rect _Rect, float _Step, bool _Time = false)
	{
		int min = Mathf.CeilToInt(m_MinTime / _Step);
		int max = Mathf.FloorToInt(m_MaxTime / _Step);
		
		if (min > max)
			return;
		
		float value = _Rect.width / (max - min + 1);
		
		if (value < 4)
			return;
		
		float scale = MathUtility.Remap(value, 40, 60, 0.25f, 0.5f);
		
		scale = Mathf.Clamp(scale, 0.25f, 0.5f);
		
		float guideAlpha = Mathf.InverseLerp(4, 8, value);
		
		Handles.color = new Color(1, 1, 1, guideAlpha);
		
		for (int step = min; step <= max; step++)
		{
			float phase = Mathf.InverseLerp(m_MinTime, m_MaxTime, step * _Step);
			
			float position = Mathf.Lerp(_Rect.xMin, _Rect.xMax, phase);
			
			Handles.DrawLine(
				new Vector3(position, _Rect.yMax - _Rect.height * scale),
				new Vector3(position, _Rect.yMax)
			);
		}
		
		Handles.color = Color.white;
		
		if (!_Time || value < 40)
			return;
		
		float timeAlpha = Mathf.InverseLerp(40, 60, value);
		
		if (Mathf.Approximately(0, timeAlpha))
			return;
		
		GUI.contentColor = new Color(1, 1, 1, timeAlpha);
		
		GUI.BeginClip(_Rect);
		
		for (int step = min - 1; step <= max; step++)
		{
			float time = step * _Step;
			
			float position = MathUtility.Remap(time, m_MinTime, m_MaxTime, 0, _Rect.width);
			
			float milliseconds = time - (int)time;
			
			GUI.Label(
				new Rect(
					position,
					_Rect.y + 5,
					80,
					_Rect.height * 0.5f
				),
				milliseconds > float.Epsilon
					? $"{(int)time / 60:00}:{(int)time % 60:00}{milliseconds:.0}"
					: $"{(int)time / 60:00}:{(int)time % 60:00}"
			);
		}
		
		GUI.EndClip();
		
		GUI.contentColor = Color.white;
	}

	void DrawTimelineSeeker(Rect _Rect)
	{
		const string controlName = "sequencer_timeline_seeker";
		
		int controlID = EditorGUIUtility.GetControlID(controlName.GetHashCode(), FocusType.Passive);
		
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				float position = MathUtility.Remap(m_Sequencer.Time, m_MinTime, m_MaxTime, 0, _Rect.width);
				
				GUI.BeginClip(
					new Rect(
						_Rect.x,
						_Rect.y,
						base.position.width - _Rect.xMin,
						base.position.height
					)
				);
				
				Handles.DrawAAConvexPolygon(
					new Vector3(position - 5, _Rect.yMin),
					new Vector3(position + 5, _Rect.yMin),
					new Vector3(position + 5, _Rect.yMax - 5),
					new Vector3(position, _Rect.yMax),
					new Vector3(position - 5, _Rect.yMax - 5)
				);
				
				Handles.DrawLine(
					new Vector3(position, _Rect.yMax),
					new Vector3(position, base.position.height)
				);
				
				GUI.EndClip();
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (!_Rect.Contains(Event.current.mousePosition))
					break;
				
				GUIUtility.hotControl = controlID;
				
				float time = MathUtility.Remap(
					Event.current.mousePosition.x,
					_Rect.xMin,
					_Rect.xMax,
					m_MinTime,
					m_MaxTime
				);
				
				time = Mathf.Max(time, 0);
				
				m_Sequencer.Stop();
				m_Sequencer.Time = time;
				if (m_Sequencer.Playing)
					m_Sequencer.Play();
				
				Event.current.Use();
				
				Repaint();
				
				break;
			}
			
			case EventType.MouseDrag:
			{
				if (GUIUtility.hotControl != controlID)
					break;
				
				Event.current.Use();
				
				float time = MathUtility.Remap(
					Event.current.mousePosition.x,
					_Rect.xMin,
					_Rect.xMax,
					m_MinTime,
					m_MaxTime
				);
				
				time = Mathf.Max(time, 0);
				
				m_Sequencer.Stop();
				m_Sequencer.Time = time;
				if (m_Sequencer.Playing)
					m_Sequencer.Play();
				
				Event.current.Use();
				
				Repaint();
				
				break;
			}
		}
	}

	void DrawTracks(Rect _Rect)
	{
		float position = 0;
		
		foreach (Track track in m_Sequencer.Tracks)
		{
			if (track == null)
				continue;
			
			if (!m_TrackObjects.ContainsKey(track) || m_TrackObjects[track] == null)
				m_TrackObjects[track] = new SerializedObject(track);
			
			SerializedObject trackObject = m_TrackObjects[track];
			
			SerializedProperty heightProperty = trackObject.FindProperty("m_Height");
			
			float height = Mathf.Clamp(heightProperty.floatValue, track.MinHeight, track.MaxHeight);
			
			DrawTrack(
				new Rect(
					_Rect.x,
					_Rect.y + position,
					_Rect.width,
					height
				),
				trackObject,
				track.MinHeight,
				track.MaxHeight
			);
			
			position += height;
		}
	}

	void DrawTrack(Rect _Rect, SerializedObject _TrackObject, float _MinHeight, float _MaxHeight)
	{
		int controlID = $"{_TrackObject.GetHashCode()}sequencer_track_height_handle".GetHashCode();
		
		RectOffset handlePadding = new RectOffset(0, 0, 100, 100);
		
		Rect handleRect = new Rect(_Rect.x, _Rect.yMax - 4, _Rect.width, 8);
		
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				EditorGUI.DrawRect(
					new RectOffset(0, 0, 1, 1).Remove(_Rect),
					new Color(0.12f, 0.12f, 0.12f)
				);
				
				GUI.Label(
					_Rect,
					_TrackObject.targetObject.name,
					EditorStyles.whiteBoldLabel
				);
				
				EditorGUIUtility.AddCursorRect(
					GUIUtility.hotControl == controlID
						? handlePadding.Add(handleRect)
						: handleRect,
					MouseCursor.SplitResizeUpDown,
					controlID
				);
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (!handleRect.Contains(Event.current.mousePosition))
					break;
				
				GUIUtility.hotControl = controlID;
				
				Event.current.Use();
				
				Repaint();
				
				break;
			}
			
			case EventType.MouseDrag:
			{
				if (GUIUtility.hotControl != controlID)
					break;
				
				SerializedProperty heightProperty = _TrackObject.FindProperty("m_Height");
				
				heightProperty.floatValue = Mathf.Clamp(heightProperty.floatValue + Event.current.delta.y, _MinHeight, _MaxHeight);
				
				_TrackObject.ApplyModifiedProperties();
				
				Event.current.Use();
				
				Repaint();
				
				break;
			}
		}
	}

	void DrawClips(Rect _Rect)
	{
		GUI.BeginClip(_Rect);
		
		float position = 0;
		
		foreach (Track track in m_Sequencer.Tracks)
		{
			if (track == null)
				continue;
			
			if (!m_TrackObjects.ContainsKey(track) || m_TrackObjects[track] == null)
				m_TrackObjects[track] = new SerializedObject(track);
			
			SerializedObject trackObject = m_TrackObjects[track];
			
			SerializedProperty heightProperty = trackObject.FindProperty("m_Height");
			
			float height = Mathf.Clamp(heightProperty.floatValue, track.MinHeight, track.MaxHeight);
			
			trackObject.UpdateIfRequiredOrScript();
			
			SerializedProperty clipsProperty = trackObject.FindProperty("m_Clips");
			
			if (clipsProperty == null)
				continue;
			
			for (int i = 0; i < clipsProperty.arraySize; i++)
			{
				DrawClip(
					new Rect(
						0,
						position,
						_Rect.width,
						height
					),
					clipsProperty.GetArrayElementAtIndex(i)
				);
			}
			
			position += height;
		}
		
		GUI.EndClip();
	}

	void DrawClip(Rect _Rect, SerializedProperty _Property)
	{
		if (_Property == null)
			return;
		
		string propertyID = $"[{_Property.serializedObject.targetObject.name}::{_Property.type}]{_Property.propertyPath}";
		
		if (!m_ClipDrawers.ContainsKey(propertyID) || m_ClipDrawers[propertyID] == null)
			m_ClipDrawers[propertyID] = ClipDrawer.Create(_Property);
		
		ClipDrawer clipDrawer = m_ClipDrawers[propertyID];
		
		clipDrawer?.Draw(_Rect, m_MinTime, m_MaxTime);
	}

	void MoveInput(Rect _Rect)
	{
		if (Event.current.type == EventType.Repaint || Event.current.type != EventType.ScrollWheel || Event.current.modifiers != EventModifiers.None)
			return;
		
		if (!_Rect.Contains(Event.current.mousePosition))
			return;
		
		Vector2 scroll = Event.current.delta;
		
		if (Mathf.Abs(scroll.x) <= Mathf.Abs(scroll.y))
			return;
		
		float scale = scroll.x / _Rect.width;
		float value = scale * Mathf.Abs(m_MaxTime - m_MinTime) * 2;
		
		float minTime = m_MinTime + value;
		float maxTime = m_MaxTime + value;
		
		ClampTimeRange(ref minTime, ref maxTime);
		
		if (!Mathf.Approximately(m_MinTime, minTime) || !Mathf.Approximately(m_MaxTime, maxTime))
		{
			m_MinTime = minTime;
			m_MaxTime = maxTime;
			
			Repaint();
		}
		
		Event.current.Use();
	}

	void ZoomInput(Rect _Rect)
	{
		if (Event.current.type == EventType.Repaint || Event.current.type != EventType.ScrollWheel || Event.current.modifiers != EventModifiers.Command)
			return;
		
		if (!_Rect.Contains(Event.current.mousePosition))
			return;
		
		Vector2 scroll = Event.current.delta;
		
		if (Mathf.Abs(scroll.y) <= Mathf.Abs(scroll.x))
			return;
		
		float phase = Mathf.InverseLerp(_Rect.xMin, _Rect.xMax, Event.current.mousePosition.x);
		float scale = scroll.y / _Rect.width;
		float value = scale * Mathf.Abs(m_MaxTime - m_MinTime) * 2;
		
		float minTime = m_MinTime - value * phase;
		float maxTime = m_MaxTime + value * (1 - phase);
		
		ClampTimeRange(ref minTime, ref maxTime);
		
		if (!Mathf.Approximately(m_MaxTime - m_MinTime, maxTime - minTime))
		{
			m_MinTime = minTime;
			m_MaxTime = maxTime;
			
			Repaint();
		}
		
		Event.current.Use();
	}

	void ResizeTracksInput(Rect _Rect)
	{
		int controlID = "sequence_editor_tracks_handle".GetHashCode();
		
		RectOffset handlePadding = new RectOffset(100, 100, 0, 0);
		
		Rect handleRect = new Rect(
			_Rect.xMax - 4,
			_Rect.y,
			8,
			_Rect.height
		);
		
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				EditorGUIUtility.AddCursorRect(
					GUIUtility.hotControl == controlID
						? handlePadding.Add(handleRect)
						: handleRect,
					MouseCursor.SplitResizeLeftRight,
					controlID
				);
				
				break;
			}
			
			case EventType.MouseDown:
			{
				if (!handleRect.Contains(Event.current.mousePosition))
					break;
				
				GUIUtility.hotControl = controlID;
				
				Event.current.Use();
				
				Repaint();
				
				break;
			}
			
			case EventType.MouseDrag:
			{
				if (GUIUtility.hotControl != controlID)
					break;
				
				TracksWidth += Event.current.delta.x;
				
				Event.current.Use();
				
				Repaint();
				
				break;
			}
		}
	}

	static void ClampTimeRange(ref float _MinTime, ref float _MaxTime)
	{
		const float minDelta = 0.25f;
		const float maxDelta = 300;
		
		float delta = Mathf.Clamp(_MaxTime - _MinTime, minDelta, maxDelta);
		
		_MinTime = Mathf.Max(0, _MinTime);
		_MaxTime = _MinTime + delta;
	}
}