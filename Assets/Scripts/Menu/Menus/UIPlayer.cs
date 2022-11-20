using System;
using System.Collections.Generic;
using System.Linq;
using AudioBox.ASF;
using AudioBox.Logging;
using Melanchall.DryWetMidi.Core;
using UnityEngine;
using Random = UnityEngine.Random;

public class UIPlayer : ASFPlayer
{
	public override double Length => m_Length;

	[SerializeField] UIAudioWave          m_Background;
	[SerializeField] UIBeat               m_Beat;
	[SerializeField] RectTransform        m_InputArea;
	[SerializeField] UIColorTrackContext  m_ColorTrack;
	[SerializeField] UITapTrackContext    m_TapTrack;
	[SerializeField] UIDoubleTrackContext m_DoubleTrack;
	[SerializeField] UIHoldTrackContext   m_HoldTrack;

	float m_Length;

	public void Setup(float _Ratio, float _Duration, AudioClip _Music, Dictionary<string, object> _ASF)
	{
		Clear();
		ClearTracks();
		
		Ratio    = _Ratio;
		Duration = _Duration;
		Music    = _Music;
		
		m_Length = Music.length;
		
		float position = 1.0f - Ratio;
		
		m_InputArea.anchorMin = new Vector2(0.5f, position);
		m_InputArea.anchorMax = new Vector2(0.5f, position);
		
		AddTrack(new ASFColorTrack(m_ColorTrack, m_ColorTrack));
		AddTrack(new ASFTapTrack(m_TapTrack));
		AddTrack(new ASFDoubleTrack(m_DoubleTrack));
		AddTrack(new ASFHoldTrack(m_HoldTrack));
		
		try
		{
			Deserialize(_ASF);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
	}

	public void Generate()
	{
		double step = 60.0d / m_Beat.BPM / m_Beat.Bar;
		
		double source = Math.Ceiling(Time / step) * step;
		double target = source + Duration * 4;
		
		ASFTapTrack    tapTrack    = GetTrack<ASFTapTrack>();
		ASFDoubleTrack doubleTrack = GetTrack<ASFDoubleTrack>();
		
		const float offset = 1.0f / 3.0f;
		
		while (source <= target)
		{
			float value = m_Background.GetMax(source);
			
			source += step;
			
			if (value < 0.4f)
				continue;
			
			Random.InitState(Mathf.CeilToInt(value * 100000));
			
			if (value >= 0.75f)
			{
				ASFDoubleClip clip = new ASFDoubleClip(source);
				
				doubleTrack.AddClip(clip);
			}
			else if (value >= 0.35f)
			{
				ASFTapClip clip = new ASFTapClip(
					source,
					offset * Random.Range(0, 4)
				);
				
				tapTrack.AddClip(clip);
			}
		}
		
		float shift = Duration * 4;
		
		Time -= shift;
		Time += shift;
	}

	public void Cleanup()
	{
		CleanupTap();
		CleanupDouble();
		CleanupHold();
		
		Sample();
	}

	void CleanupTap()
	{
		double step = 60.0d / m_Beat.BPM / m_Beat.Bar / 4;
		
		const float position = 0.1f;
		
		ASFTapTrack track = GetTrack<ASFTapTrack>();
		
		if (track == null || track.Clips == null)
			return;
		
		track.SortClips();
		
		List<ASFTapClip> clips = new List<ASFTapClip>();
		for (int i = 0; i < track.Clips.Count; i++)
		for (int j = i + 1; j < track.Clips.Count; j++)
		{
			ASFTapClip a = track.Clips[i];
			ASFTapClip b = track.Clips[j];
			
			if (Math.Abs(a.Time - b.Time) > step)
				continue;
			
			if (Math.Abs(a.Position - b.Position) > position)
				continue;
			
			clips.Add(b);
		}
		
		foreach (ASFTapClip clip in clips.Distinct())
			track.RemoveClip(clip);
	}

	void CleanupDouble()
	{
		double step = 60.0d / m_Beat.BPM / m_Beat.Bar / 4;
		
		ASFDoubleTrack track = GetTrack<ASFDoubleTrack>();
		
		if (track == null || track.Clips == null)
			return;
		
		track.SortClips();
		
		List<ASFDoubleClip> clips = new List<ASFDoubleClip>();
		for (int i = 0; i < track.Clips.Count; i++)
		for (int j = i + 1; j < track.Clips.Count; j++)
		{
			ASFDoubleClip a = track.Clips[i];
			ASFDoubleClip b = track.Clips[j];
			
			if (Math.Abs(a.Time - b.Time) > step)
				continue;
			
			clips.Add(b);
		}
		
		foreach (ASFDoubleClip clip in clips.Distinct())
			track.RemoveClip(clip);
	}

	void CleanupHold()
	{
		double step = 60.0d / m_Beat.BPM / m_Beat.Bar / 4;
		
		const float position = 0.1f;
		
		ASFHoldTrack track = GetTrack<ASFHoldTrack>();
		
		if (track == null || track.Clips == null)
			return;
		
		track.SortClips();
		
		List<ASFHoldClip> clips = new List<ASFHoldClip>();
		for (int i = 0; i < track.Clips.Count; i++)
		for (int j = i + 1; j < track.Clips.Count; j++)
		{
			ASFHoldClip a = track.Clips[i];
			ASFHoldClip b = track.Clips[j];
			
			if (Math.Abs(a.MinTime - b.MinTime) > step)
				continue;
			
			if (Math.Abs(a.MaxTime - b.MaxTime) > step)
				continue;
			
			ASFHoldKey aFirst = a.Keys.FirstOrDefault();
			ASFHoldKey bFirst = b.Keys.FirstOrDefault();
			
			if (aFirst == null || bFirst == null || Math.Abs(aFirst.Position - bFirst.Position) > position)
				continue;
			
			ASFHoldKey aLast = a.Keys.LastOrDefault();
			ASFHoldKey bLast = b.Keys.LastOrDefault();
			
			if (aLast == null || bLast == null || Math.Abs(aLast.Position - bLast.Position) > position)
				continue;
			
			clips.Add(b);
		}
		
		foreach (ASFHoldClip clip in clips.Distinct())
			track.RemoveClip(clip);
	}

	public void Deserialize(MidiFile _Midi)
	{
		if (_Midi == null)
			return;
		
		Clear();
		ClearTracks();
		
		ASFTapTrack    tapTrack    = new ASFTapTrack(m_TapTrack);
		ASFDoubleTrack doubleTrack = new ASFDoubleTrack(m_DoubleTrack);
		ASFHoldTrack   holdTrack   = new ASFHoldTrack(m_HoldTrack);
		
		foreach (ASFTapClip tapClip in _Midi.GetTapClips())
			tapTrack.AddClip(tapClip);
		
		foreach (ASFDoubleClip doubleClip in _Midi.GetDoubleClips())
			doubleTrack.AddClip(doubleClip);
		
		foreach (ASFHoldClip holdClip in _Midi.GetHoldClips())
			holdTrack.AddClip(holdClip);
		
		foreach (ASFHoldClip bendClip in _Midi.GetBendClips(1))
			holdTrack.AddClip(bendClip);
		
		foreach (ASFHoldClip bendClip in _Midi.GetBendClips(2))
			holdTrack.AddClip(bendClip);
		
		AddTrack(tapTrack);
		AddTrack(doubleTrack);
		AddTrack(holdTrack);
		
		Sample();
	}

	public void Clear()
	{
		m_TapTrack.Clear();
		m_DoubleTrack.Clear();
		m_HoldTrack.Clear();
		m_ColorTrack.Clear();
	}

	public override void Sample()
	{
		base.Sample();
		
		if (m_Background != null)
			m_Background.Time = Time;
		
		if (m_Beat != null)
			m_Beat.Time = Time;
		
		if (Time >= m_Length)
			Stop();
	}
}
