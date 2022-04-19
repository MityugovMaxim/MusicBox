using System;
using AudioBox.ASF;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class SongPlayer : ASFPlayer
{
	[Preserve]
	public class Factory : PlaceholderFactory<SongPlayer, SongPlayer> { }

	[SerializeField] UITapTrack      m_TapTrack;
	[SerializeField] UIDoubleTrack   m_DoubleTrack;
	[SerializeField] UIHoldTrack     m_HoldTrack;
	[SerializeField] UIColorTrack    m_ColorTrack;
	[SerializeField] RectTransform   m_InputArea;
	[SerializeField] UIInputReceiver m_InputReceiver;

	Action m_Finished;
	double m_FinishTime;

	public void Setup(float _Ratio, float _Duration, AudioClip _Music, string _ASF, Action _Finished)
	{
		Ratio    = _Ratio;
		Duration = _Duration;
		Music    = _Music;
		
		m_Finished   = _Finished;
		m_FinishTime = Music.length + Duration * Ratio;
		
		m_InputArea.anchorMin = new Vector2(0, 1.0f - Ratio);
		m_InputArea.anchorMax = new Vector2(1, 1.0f - Ratio);
		
		AddTrack(new ASFTapTrack(m_TapTrack));
		AddTrack(new ASFDoubleTrack(m_DoubleTrack));
		AddTrack(new ASFHoldTrack(m_HoldTrack));
		AddTrack(new ASFColorTrack(m_ColorTrack, m_ColorTrack));
		
		Deserialize(_ASF);
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
		
		#if UNITY_EDITOR
		if (Music == null)
			return;
		#endif
		
		if (Time >= m_FinishTime)
			m_Finished?.Invoke();
	}
}