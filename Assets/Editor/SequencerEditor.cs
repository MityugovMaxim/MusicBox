using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SequencerEditor : EditorWindow
{
	[MenuItem("Window/Sequencer")]
	public void Open()
	{
		SequencerEditor window = GetWindow<SequencerEditor>();
		window.minSize = new Vector2(300, 300);
	}

	readonly Dictionary<Clip, ClipDrawer> m_ClipDrawers = new Dictionary<Clip, ClipDrawer>();

	[SerializeField] Sequencer m_Sequencer;
	[SerializeField] float     m_MinTime;
	[SerializeField] float     m_MaxTime;

	void OnSelectionChange()
	{
		Sequencer sequencer = Selection.GetFiltered<Sequencer>(SelectionMode.Assets).FirstOrDefault();
		
		if (sequencer == null)
			return;
		
		m_Sequencer = sequencer;
		m_MinTime   = 0;
		m_MaxTime   = 60;
		
		foreach (Track track in m_Sequencer.Tracks)
		foreach (Clip clip in track)
			m_MaxTime = Mathf.Max(m_MaxTime, clip.FinishTime);
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
		DrawTimelineMinutes(_Rect);
		
		DrawTimelineSeconds(_Rect);
		
		DrawTimelineSeeker(_Rect);
	}

	void DrawTimelineMinutes(Rect _Rect)
	{
		const int step = 60;
		
		int min = Mathf.CeilToInt(m_MinTime / step);
		int max = Mathf.FloorToInt(m_MaxTime / step);
		
		for (int minute = min; minute <= max; minute++)
		{
			float phase = Mathf.InverseLerp(m_MinTime, m_MaxTime, minute * step);
			
			float position = Mathf.Lerp(_Rect.xMin, _Rect.xMax, phase);
			
			Handles.DrawLine(
				new Vector3(_Rect.x + position, _Rect.yMin),
				new Vector3(_Rect.x + position, _Rect.yMax)
			);
		}
	}

	void DrawTimelineSeconds(Rect _Rect)
	{
		int min = Mathf.CeilToInt(m_MinTime);
		int max = Mathf.FloorToInt(m_MaxTime);
		
		for (int second = min; second <= max; second++)
		{
			if (second % 60 == 0)
				continue;
			
			float phase = Mathf.InverseLerp(m_MinTime, m_MaxTime, second);
			
			float position = Mathf.Lerp(_Rect.xMin, _Rect.xMax, phase);
			
			Handles.DrawLine(
				new Vector3(_Rect.x + position, _Rect.yMin),
				new Vector3(_Rect.x + position, _Rect.yMax)
			);
		}
	}

	void DrawTimelineSeeker(Rect _Rect)
	{
		
	}

	void DrawTracks(Rect _Rect)
	{
		const float height = 60;
		
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
		const float height = 60;
		
		Rect rect = new Rect(_Rect.x, _Rect.y, _Rect.width, height);
		
		foreach (Track track in m_Sequencer.Tracks)
		{
			foreach (Clip clip in track)
				DrawClip(rect, clip);
			
			rect.y += height;
		}
	}

	void DrawClip(Rect _Rect, Clip _Clip)
	{
		if (m_MinTime >= _Clip.FinishTime || m_MaxTime <= _Clip.StartTime)
			return;
		
		float min = Mathf.InverseLerp(m_MinTime, m_MaxTime, _Clip.StartTime);
		float max = Mathf.InverseLerp(m_MinTime, m_MaxTime, _Clip.FinishTime);
		
		Rect rect = new Rect(
			_Rect.x + min,
			_Rect.y,
			max - min,
			_Rect.height
		);
		
		if (!m_ClipDrawers.ContainsKey(_Clip) || m_ClipDrawers[_Clip] == null)
			m_ClipDrawers[_Clip] = ClipDrawer.Create(_Clip);
		
		ClipDrawer clipDrawer = m_ClipDrawers[_Clip];
		
		if (clipDrawer != null)
			clipDrawer.Draw(rect);
	}
}