using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[Serializable]
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

	Sequencer Sequencer
	{
		get
		{
			if (m_Sequencer == null)
				m_Sequencer = EditorUtility.InstanceIDToObject(m_SequencerID) as Sequencer;
			return m_Sequencer;
		}
	}

	static readonly Dictionary<int, TrackDrawer> m_TrackDrawers = new Dictionary<int, TrackDrawer>();
	static readonly Dictionary<int, ClipDrawer>  m_ClipDrawers  = new Dictionary<int, ClipDrawer>();

	[SerializeField] int   m_SequencerID;
	[SerializeField] float m_MinTime;
	[SerializeField] float m_MaxTime;

	Sequencer m_Sequencer;

	void OnEnable()
	{
		Undo.undoRedoPerformed += Repaint;
	}

	void OnDisable()
	{
		Undo.undoRedoPerformed -= Repaint;
	}

	void OnInspectorUpdate()
	{
		Repaint();
	}

	void OnHierarchyChange()
	{
		Repaint();
	}

	void OnSelectionChange()
	{
		Sequencer sequencer = Selection.GetFiltered<Sequencer>(SelectionMode.Assets).FirstOrDefault();
		
		if (sequencer == null)
			return;
		
		m_TrackDrawers.Clear();
		m_ClipDrawers.Clear();
		
		m_Sequencer   = sequencer;
		m_SequencerID = m_Sequencer.GetInstanceID();
		
		if (Mathf.Approximately(m_MinTime, m_MaxTime))
		{
			m_MinTime = 0;
			m_MaxTime = 60;
		}
		
		if (!Application.isPlaying)
			Sequencer.Initialize();
		
		foreach (Track track in Sequencer.Tracks)
		foreach (Clip clip in track)
			m_MaxTime = Mathf.Max(m_MaxTime, clip.MaxTime);
		
		Repaint();
	}

	void OnGUI()
	{
		if (Sequencer == null)
			return;
		
		Rect toolbarRect = new Rect(0, 0, TracksWidth, 25);
		
		Rect timelineRect = new Rect(TracksWidth, 0, position.width - TracksWidth, 25);
		
		Rect tracksRect = new Rect(0, 25, TracksWidth, position.height - 25);
		
		Rect clipsRect = new Rect(TracksWidth, 25, position.width - TracksWidth, position.height - 25);
		
		DrawTracks(tracksRect);
		DrawClips(clipsRect);
		DrawToolbar(toolbarRect);
		DrawTimeline(timelineRect);
		
		CopyInput();
		PasteInput();
		MoveInput(clipsRect);
		ZoomInput(clipsRect);
		DragDropInput(clipsRect);
		ResizeTracksInput(tracksRect);
	}

	void DrawToolbar(Rect _Rect)
	{
		GUILayout.BeginArea(_Rect);
		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button(">"))
		{
			Sequencer.Initialize();
			Sequencer.Play();
		}
		
		if (GUILayout.Button("||"))
			Sequencer.Pause();
		
		if (GUILayout.Button("[]"))
			Sequencer.Stop();
		
		GUILayout.EndHorizontal();
		
		GUILayout.EndArea();
	}

	void Update()
	{
		if (Sequencer != null && Sequencer.Playing)
			EditorApplication.QueuePlayerLoopUpdate();
		
		Repaint();
	}

	void DrawTimeline(Rect _Rect)
	{
		DrawTimelineGuide(_Rect, 0.01f, false);
		DrawTimelineGuide(_Rect, 0.1f, true);
		DrawTimelineGuide(_Rect, 1, true);
		DrawTimelineGuide(_Rect, 5, true);
		DrawTimelineGuide(_Rect, 20, true);
		DrawTimelineGuide(_Rect, 60, true);
		
		DrawTimelineSeeker(_Rect);
	}

	void DrawTimelineGuide(Rect _Rect, float _Step, bool _Time)
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
		
		Color color = Handles.color;
		color.a = Mathf.InverseLerp(4, 8, value);
		Handles.color = color;
		
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

	void DrawTimelineGuide(Rect _Rect, float _Step)
	{
		int min = Mathf.CeilToInt(m_MinTime / _Step);
		int max = Mathf.FloorToInt(m_MaxTime / _Step);
		
		if (min > max)
			return;
		
		float value = _Rect.width / (max - min + 1);
		
		if (value < 4)
			return;
		
		Color color = Handles.color;
		float alpha = color.a;
		color.a = Mathf.Lerp(0, 0.5f, Mathf.InverseLerp(4, 8, value));
		
		Handles.color = color;
		
		for (int step = min; step <= max; step++)
		{
			float phase = Mathf.InverseLerp(m_MinTime, m_MaxTime, step * _Step);
			
			float position = Mathf.Lerp(_Rect.xMin, _Rect.xMax, phase);
			
			Handles.DrawLine(
				new Vector3(position, _Rect.yMin),
				new Vector3(position, _Rect.yMax)
			);
		}
		
		color.a = alpha;
		Handles.color = color;
	}

	void DrawTimelineSeeker(Rect _Rect)
	{
		const string controlName = "sequencer_timeline_seeker";
		
		int controlID = EditorGUIUtility.GetControlID(controlName.GetHashCode(), FocusType.Passive);
		
		switch (Event.current.type)
		{
			case EventType.Repaint:
			{
				float position = MathUtility.Remap(Sequencer.Time, m_MinTime, m_MaxTime, 0, _Rect.width);
				
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
				
				if (!Event.current.alt)
					time = MathUtility.Snap(time, 0.01f);
				time = Mathf.Max(time, 0);
				
				bool playing = Sequencer.Playing;
				
				Sequencer.Stop();
				Sequencer.Time = time;
				if (playing)
					Sequencer.Play();
				
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
				
				if (!Event.current.alt)
					time = MathUtility.Snap(time, 0.01f);
				time = Mathf.Max(time, 0);
				
				bool playing = Sequencer.Playing;
				
				Sequencer.Stop();
				Sequencer.Time = time;
				if (playing)
					Sequencer.Play();
				
				Event.current.Use();
				
				Repaint();
				
				break;
			}
		}
	}

	void DrawTracks(Rect _Rect)
	{
		float position = 0;
		
		RectOffset trackPadding = new RectOffset(0, 0, 1, 1);
		
		foreach (Track track in Sequencer.Tracks)
		{
			if (track == null)
				continue;
			
			Rect rect = new Rect(
				_Rect.x,
				_Rect.y + position,
				_Rect.width,
				track.Height
			);
			
			DrawTrack(trackPadding.Remove(rect), track);
			
			position += track.Height;
		}
	}

	void DrawTrack(Rect _Rect, Track _Track)
	{
		if (_Track == null || Event.current.type == EventType.Used)
			return;
		
		int trackID = _Track.GetInstanceID();
		
		if (!m_TrackDrawers.ContainsKey(trackID) || m_TrackDrawers[trackID] == null)
			m_TrackDrawers[trackID] = TrackDrawer.Create(_Track);
		
		TrackDrawer trackDrawer = m_TrackDrawers[trackID];
		
		trackDrawer?.Draw(_Rect, Sequencer.Time);
	}

	void DrawClips(Rect _Rect)
	{
		Handles.color = new Color(0.1f, 0.1f, 0.1f);
		DrawTimelineGuide(_Rect, 0.01f);
		DrawTimelineGuide(_Rect, 0.1f);
		DrawTimelineGuide(_Rect, 1);
		DrawTimelineGuide(_Rect, 5);
		DrawTimelineGuide(_Rect, 20);
		DrawTimelineGuide(_Rect, 60);
		Handles.color = Color.white;
		
		GUI.BeginClip(_Rect);
		
		float position = 0;
		
		RectOffset trackPadding = new RectOffset(0, 0, 2, 2);
		RectOffset clipPadding  = new RectOffset(0, 0, 4, 4);
		
		foreach (Track track in Sequencer.Tracks)
		{
			if (track == null)
				continue;
			
			Rect rect = new Rect(
				0,
				position,
				_Rect.width,
				track.Height
			);
			
			switch (Event.current.type)
			{
				case EventType.Repaint:
				{
					Handles.DrawSolidRectangleWithOutline(
						trackPadding.Remove(rect),
						new Color(0.7f, 0.7f, 0.7f, 0.05f),
						new Color(0.4f, 0.4f, 0.4f, 0.8f)
					);
					
					break;
				}
			}
			
			foreach (Clip clip in track)
			{
				DrawClip(clipPadding.Remove(rect), clip);
			}
			
			position += track.Height;
		}
		
		GUI.EndClip();
	}

	void DrawClip(Rect _Rect, Clip _Clip)
	{
		if (_Clip == null || Event.current.type == EventType.Used)
			return;
		
		int clipID = _Clip.GetInstanceID();
		
		if (!m_ClipDrawers.ContainsKey(clipID) || m_ClipDrawers[clipID] == null)
			m_ClipDrawers[clipID] = ClipDrawer.Create(_Clip);
		
		ClipDrawer clipDrawer = m_ClipDrawers[clipID];
		
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

	void CopyInput()
	{
		switch (Event.current.type)
		{
			case EventType.ValidateCommand:
				if (Event.current.commandName == "Copy")
					Event.current.Use();
				break;
			
			case EventType.ExecuteCommand:
			{
				if (Event.current.commandName != "Copy")
					break;
				
				const string header = "sequencer_clips";
				
				Clip[] clips = Selection.GetFiltered<Clip>(SelectionMode.Unfiltered);
				
				StringBuilder data = new StringBuilder();
				
				data.AppendLine(header);
				
				foreach (Clip clip in clips)
				{
					string path = AssetDatabase.GetAssetPath(clip);
					string guid = AssetDatabase.AssetPathToGUID(path);
					string type = clip.GetType().Name;
					string json = JsonUtility.ToJson(clip);
					
					data.Append(guid)
						.Append(';')
						.Append(type)
						.Append(';')
						.Append(json)
						.AppendLine();
				}
				
				GUIUtility.systemCopyBuffer = data.ToString();
				
				Event.current.Use();
				
				break;
			}
		}
	}

	void PasteInput()
	{
		switch (Event.current.type)
		{
			case EventType.ValidateCommand:
				if (Event.current.commandName == "Paste" && Selection.GetFiltered<Clip>(SelectionMode.Assets).Length > 0)
					Event.current.Use();
				break;
			
			case EventType.ExecuteCommand:
			{
				if (Event.current.commandName != "Paste")
					break;
				
				const string header = "sequencer_clips";
				
				if (!GUIUtility.systemCopyBuffer.StartsWith(header))
					break;
				
				string[] clipsData = GUIUtility.systemCopyBuffer.Split('\n');
				
				for (int i = 1; i < clipsData.Length; i++)
				{
					string clipData = clipsData[i];
					
					string[] clipParameters = clipData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					
					string guidParameter = clipParameters[0];
					string typeParameter = clipParameters[1];
					string jsonParameter = clipParameters[2];
					
					string path  = AssetDatabase.GUIDToAssetPath(guidParameter);
					Track  track = AssetDatabase.LoadMainAssetAtPath(path) as Track;
					
					if (track == null || !Sequencer.Tracks.Contains(track))
						continue;
					
					Type type = Type.GetType(typeParameter);
					Clip clip = JsonUtility.FromJson(jsonParameter, type) as Clip;
					
					if (clip == null)
						continue;
					
					Debug.LogError(clip.name);
					
					TrackUtility.AddClip(track, clip, Sequencer.Time);
				}
				
				Event.current.Use();
				
				Repaint();
				
				break;
			}
		}
	}

	void DragDropInput(Rect _Rect)
	{
		float position = 0;
		
		foreach (Track track in Sequencer.Tracks)
		{
			if (track == null)
				continue;
			
			Rect rect = new Rect(
				_Rect.x,
				_Rect.y + position,
				_Rect.width,
				track.Height
			);
			
			switch (Event.current.type)
			{
				case EventType.DragUpdated:
				{
					if (!rect.Contains(Event.current.mousePosition))
						break;
					
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					
					break;
				}
				
				case EventType.DragPerform:
				{
					if (!rect.Contains(Event.current.mousePosition))
						break;
					
					float time = MathUtility.Remap(
						Event.current.mousePosition.x,
						rect.xMin,
						rect.xMax,
						m_MinTime,
						m_MaxTime
					);
					
					track.DragPerform(time, DragAndDrop.objectReferences);
					
					DragAndDrop.AcceptDrag();
					
					Event.current.Use();
					
					Repaint();
					
					break;
				}
			}
			
			position += track.Height;
		}
	}

	void ResizeTracksInput(Rect _Rect)
	{
		int controlID = EditorGUIUtility.GetControlID("sequence_editor_tracks_handle".GetHashCode(), FocusType.Passive);
		
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