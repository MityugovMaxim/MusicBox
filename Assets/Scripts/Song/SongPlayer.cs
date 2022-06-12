using System;
using System.Linq;
using AudioBox.ASF;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class SongPlayer : ASFPlayer
{
	[Preserve]
	public class Factory : PlaceholderFactory<SongPlayer, SongPlayer> { }

	public override double Length => m_Length;

	[SerializeField] UITapTrack      m_TapTrack;
	[SerializeField] UIDoubleTrack   m_DoubleTrack;
	[SerializeField] UIHoldTrack     m_HoldTrack;
	[SerializeField] UIColorTrack    m_ColorTrack;
	[SerializeField] RectTransform   m_InputArea;
	[SerializeField] UIInputReceiver m_InputReceiver;
	[SerializeField] UICountdown     m_Countdown;

	Action m_Finished;
	double m_Length;

	public void Setup(float _Ratio, float _Duration, AudioClip _Music, string _ASF, Action _Finished)
	{
		Ratio    = _Ratio;
		Duration = _Duration;
		Music    = _Music;
		
		float position = 1.0f - Ratio;
		
		m_Finished = _Finished;
		
		m_InputArea.anchorMin = new Vector2(0, position);
		m_InputArea.anchorMax = new Vector2(1, position);
		
		AddTrack(new ASFTapTrack(m_TapTrack));
		AddTrack(new ASFDoubleTrack(m_DoubleTrack));
		AddTrack(new ASFHoldTrack(m_HoldTrack));
		AddTrack(new ASFColorTrack(m_ColorTrack, m_ColorTrack));
		
		Deserialize(_ASF);
		
		m_Length = GetLength() + Duration * position;
	}

	double GetLength()
	{
		double time = 0;
		
		double tapTime = GetTrack<ASFTapTrack>().Clips
			.Select(_Clip => _Clip.MaxTime)
			.DefaultIfEmpty(0)
			.Max();
		
		double doubleTime = GetTrack<ASFTapTrack>().Clips
			.Select(_Clip => _Clip.MaxTime)
			.DefaultIfEmpty(0)
			.Max();
		
		double holdTime = GetTrack<ASFHoldTrack>().Clips
			.Select(_Clip => _Clip.MaxTime)
			.DefaultIfEmpty(0)
			.Max();
		
		if (tapTime > time)
			time = tapTime;
		
		if (doubleTime > time)
			time = doubleTime;
		
		if (holdTime > time)
			time = holdTime;
		
		if (time > Music.length)
			time = Music.length;
		
		return time > double.Epsilon ? time : Music.length;
	}

	public override void Play(float _Latency)
	{
		base.Play(_Latency);
		
		m_Countdown.Play();
	}

	public override void Stop()
	{
		base.Stop();
		
		m_InputReceiver.Release();
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
		
		m_InputReceiver.Process();
		
		if (Time >= m_Length && State == ASFPlayerState.Play)
			m_Finished?.Invoke();
	}
}