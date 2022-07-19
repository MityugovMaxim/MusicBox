using System;
using AudioBox.ASF;
using AudioBox.Logging;
using Melanchall.DryWetMidi.Core;
using UnityEngine;

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

	public void Setup(float _Ratio, float _Duration, AudioClip _Music, string _ASF)
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