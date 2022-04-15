using System;
using System.Threading.Tasks;
using AudioBox.ASF;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class TutorialPlayer : ASFPlayer
{
	[Preserve]
	public class Factory : PlaceholderFactory<TutorialPlayer, TutorialPlayer> { }

	[SerializeField] UITapTrack      m_TapTrack;
	[SerializeField] UIDoubleTrack   m_DoubleTrack;
	[SerializeField] UIHoldTrack     m_HoldTrack;
	[SerializeField] UIColorTrack    m_ColorTrack;
	[SerializeField] RectTransform   m_InputArea;
	[SerializeField] UIInputReceiver m_InputReceiver;

	[SerializeField] UIGroup m_InputZoneGroup;
	[SerializeField] UIGroup m_ComboIndicatorGroup;

	public async Task Process()
	{
		ClearTracks();
		CreateColorTrack();
		CreateTapTrack();
		
		Time = -Duration * Ratio;
		
		double source = Time;
		double target = 6;
		
		await UnityTask.Phase(
			_Phase => Time = ASFMath.Lerp(source, target, _Phase),
			(float)(target - source)
		);
	}

	void CreateColorTrack()
	{
		m_ColorTrack.Clear();
		
		ASFColorTrack track = new ASFColorTrack(m_ColorTrack, m_ColorTrack);
		
		track.AddClip(new ASFColorClip(0.0d, Color.cyan, Color.magenta, Color.white, Color.blue));
		
		AddTrack(track);
	}

	void CreateTapTrack()
	{
		const float step = 1.0f / 3.0f;
		
		m_TapTrack.Clear();
		
		ASFTapTrack track = new ASFTapTrack(m_TapTrack);
		
		track.AddClip(new ASFTapClip(0.0d, step * 3));
		track.AddClip(new ASFTapClip(1.5d, step * 2));
		track.AddClip(new ASFTapClip(3.0d, step * 1));
		track.AddClip(new ASFTapClip(4.5d, step * 0));
		
		AddTrack(track);
	}

	void CreateDoubleTrack()
	{
		m_DoubleTrack.Clear();
		
		ASFDoubleTrack track = new ASFDoubleTrack(m_DoubleTrack);
		
		track.AddClip(new ASFDoubleClip(14.0d));
		track.AddClip(new ASFDoubleClip(15.5d));
		track.AddClip(new ASFDoubleClip(17.0d));
		track.AddClip(new ASFDoubleClip(18.5d));
		
		AddTrack(track);
	}

	void CreateHoldTrack()
	{
		const float step = 1.0f / 3.0f;
		
		m_HoldTrack.Clear();
		
		ASFHoldTrack track = new ASFHoldTrack(m_HoldTrack);
		
		track.AddClip(new ASFHoldClip(21.0d, 23.0d, new ASFHoldClip.Key(0.0d, step * 3), new ASFHoldClip.Key(2.0d, step * 2)));
		track.AddClip(new ASFHoldClip(24.5d, 26.5d, new ASFHoldClip.Key(0.0d, step * 0), new ASFHoldClip.Key(2.0d, step * 1)));
		
		AddTrack(track);
	}
}