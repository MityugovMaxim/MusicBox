using AudioBox.ASF;
using UnityEngine;

public class UIPlayer : ASFPlayer
{
	[SerializeField] UIAudioWave          m_Background;
	[SerializeField] UIBeat               m_Beat;
	[SerializeField] RectTransform        m_InputArea;
	[SerializeField] UIColorTrackContext  m_ColorTrack;
	[SerializeField] UITapTrackContext    m_TapTrack;
	[SerializeField] UIDoubleTrackContext m_DoubleTrack;
	[SerializeField] UIHoldTrackContext   m_HoldTrack;

	public void Setup(float _Ratio, float _Duration, AudioClip _Music, string _ASF)
	{
		Ratio    = _Ratio;
		Duration = _Duration;
		Music    = _Music;
		
		float position = 1.0f - Ratio;
		
		m_InputArea.anchorMin = new Vector2(0.5f, position);
		m_InputArea.anchorMax = new Vector2(0.5f, position);
		
		AddTrack(new ASFColorTrack(m_ColorTrack, m_ColorTrack));
		AddTrack(new ASFTapTrack(m_TapTrack));
		AddTrack(new ASFDoubleTrack(m_DoubleTrack));
		AddTrack(new ASFHoldTrack(m_HoldTrack));
		
		Deserialize(_ASF);
	}

	public void Clear()
	{
		m_TapTrack.Clear();
	}

	public override void Sample()
	{
		base.Sample();
		
		if (m_Background != null)
			m_Background.Time = Time;
		
		if (m_Beat != null)
			m_Beat.Time = Time;
		
		if (Music != null && Time >= Music.length)
			Stop();
	}
}