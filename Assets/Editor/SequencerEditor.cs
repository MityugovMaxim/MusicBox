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

	static readonly Dictionary<Clip, ClipDrawer> m_ClipDrawers = new Dictionary<Clip, ClipDrawer>();

	[SerializeField] Sequencer m_Sequencer;
	[SerializeField] float     m_MinTime;
	[SerializeField] float     m_MaxTime;

	void OnSelectionChange()
	{
		Sequencer sequencer = Selection.GetFiltered<Sequencer>(SelectionMode.Assets).FirstOrDefault();
		
		if (sequencer == null)
			return;
		
		m_ClipDrawers.Clear();
		
		m_Sequencer = sequencer;
		m_MinTime   = 0;
		m_MaxTime   = 60;
		
		foreach (Track track in m_Sequencer.Tracks)
		foreach (Clip clip in track)
			m_MaxTime = Mathf.Max(m_MaxTime, clip.FinishTime);
		
		Repaint();
	}

	void OnGUI()
	{
		if (m_Sequencer == null)
			return;
		
		Rect toolbarRect = new Rect(0, 0, 200, 40);
		
		Rect timelineRect = new Rect(200, 0, position.width - 200, 40);
		
		Rect tracksRect = new Rect(0, 40, 200, position.height - 40);
		
		Rect clipsRect = new Rect(200, 40, position.width - 200, position.height - 40);
		
		DrawToolbar(toolbarRect);
		DrawTimeline(timelineRect);
		DrawTracks(tracksRect);
		DrawClips(clipsRect);
		
		MoveInput(clipsRect);
		ZoomInput(clipsRect);
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

	void DrawTimeline(Rect _Rect)
	{
		DrawTimelineGuide(_Rect, 0.01f, 0.125f);
		
		DrawTimelineGuide(_Rect, 0.1f, 0.125f);
		
		DrawTimelineGuide(_Rect, 1, 0.25f);
		
		DrawTimelineGuide(_Rect, 5, 0.5f);
		
		DrawTimelineGuide(_Rect, 20, 0.5f);
		
		DrawTimelineGuide(_Rect, 60, 0.5f);
		
		DrawTimelineSeeker(_Rect);
	}

	void DrawTimelineGuide(Rect _Rect, float _Step, float _Scale)
	{
		int min = Mathf.CeilToInt(m_MinTime / _Step);
		int max = Mathf.FloorToInt(m_MaxTime / _Step);
		
		if (min > max)
			return;
		
		float value = _Rect.width / (max - min + 1);
		
		if (value < 2)
			return;
		
		Handles.color = new Color(1, 1, 1, Mathf.InverseLerp(2, 5, value));
		
		for (int second = min; second <= max; second++)
		{
			float phase = Mathf.InverseLerp(m_MinTime, m_MaxTime, second * _Step);
			
			float position = Mathf.Lerp(_Rect.xMin, _Rect.xMax, phase);
			
			Handles.DrawLine(
				new Vector3(position, _Rect.yMax - _Rect.height * _Scale),
				new Vector3(position, _Rect.yMax)
			);
		}
		
		Handles.color = Color.white;
	}

	void DrawTimelineMilliseconds(Rect _Rect)
	{
		float alpha = Mathf.Max(0, 1 - (m_MaxTime - m_MinTime) / 50);
		
		if (Mathf.Approximately(alpha, 0))
			return;
		
		int min = Mathf.CeilToInt(m_MinTime * 10);
		int max = Mathf.FloorToInt(m_MaxTime * 10);
		
		Handles.color = new Color(1, 1, 1, alpha);
		
		for (int millisecond = min; millisecond <= max; millisecond++)
		{
			float phase = Mathf.InverseLerp(m_MinTime * 10, m_MaxTime * 10, millisecond);
			
			float position = Mathf.Lerp(_Rect.xMin, _Rect.xMax, phase);
			
			Handles.DrawLine(
				new Vector3(position, _Rect.yMax - _Rect.height * 0.15f),
				new Vector3(position, _Rect.yMax)
			);
		}
		
		Handles.color = Color.white;
	}

	void DrawTimelineSeeker(Rect _Rect)
	{
		
	}

	void DrawTracks(Rect _Rect)
	{
		const float height = 120;
		
		Rect rect = new Rect(_Rect.x, _Rect.y, _Rect.width, height);
		
		foreach (Track track in m_Sequencer.Tracks)
		{
			DrawTrack(rect, track);
			
			rect.y += height;
		}
	}

	void DrawTrack(Rect _Rect, Track _Track)
	{
		EditorGUI.DrawRect(_Rect, Color.blue);
		
		GUI.Label(_Rect, _Track.name);
	}

	void DrawClips(Rect _Rect)
	{
		GUI.BeginClip(_Rect);
		
		const float height = 120;
		
		Rect rect = new Rect(0, 0, _Rect.width, height);
		
		foreach (Track track in m_Sequencer.Tracks)
		{
			foreach (Clip clip in track)
				DrawClip(rect, clip);
			
			rect.y += height;
		}
		
		GUI.EndClip();
	}

	void DrawClip(Rect _Rect, Clip _Clip)
	{
		if (m_MinTime >= _Clip.FinishTime || m_MaxTime <= _Clip.StartTime)
			return;
		
		float min = MathUtility.Remap01(_Clip.StartTime, m_MinTime, m_MaxTime);
		float max = MathUtility.Remap01(_Clip.FinishTime, m_MinTime, m_MaxTime);
		
		Rect rect = new Rect(
			_Rect.x + _Rect.width * min,
			_Rect.y,
			_Rect.width * (max - min),
			_Rect.height
		);
		
		min = Mathf.Max(0, min);
		max = Mathf.Min(1, max);
		
		Rect r = new Rect(
			_Rect.x + _Rect.width * min,
			_Rect.y,
			_Rect.width * (max - min),
			_Rect.height
		);
		
		if (!m_ClipDrawers.ContainsKey(_Clip) || m_ClipDrawers[_Clip] == null)
			m_ClipDrawers[_Clip] = ClipDrawer.Create(_Clip);
		
		ClipDrawer clipDrawer = m_ClipDrawers[_Clip];
		
		clipDrawer?.Draw(rect, r);
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

	static void ClampTimeRange(ref float _MinTime, ref float _MaxTime)
	{
		const float minDelta = 1;
		const float maxDelta = 300;
		
		float delta = Mathf.Clamp(_MaxTime - _MinTime, minDelta, maxDelta);
		
		_MinTime = Mathf.Max(0, _MinTime);
		_MaxTime = _MinTime + delta;
	}
}