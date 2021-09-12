using System;
using UnityEngine;

#if UNITY_EDITOR
public partial class DoubleTrack
{
	protected override float MinHeight => 50;
	protected override float MaxHeight => 50;
}
#endif

[CreateAssetMenu(fileName = "Double Track", menuName = "Tracks/Double Track")]
public partial class DoubleTrack : Track<DoubleClip>
{
	UIDoubleTrack Track
	{
		get
		{
			if (m_TrackReference == null || m_TrackCache != m_Track)
			{
				m_TrackReference = GetReference<UIDoubleTrack>(m_Track);
				m_TrackCache     = m_Track;
			}
			return m_TrackReference;
		}
	}

	[SerializeField, Reference(typeof(UIDoubleTrack))] string m_Track;

	[NonSerialized] string        m_TrackCache;
	[NonSerialized] UIDoubleTrack m_TrackReference;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		if (Track == null)
		{
			Debug.LogError($"[TapTrack] Track is not assigned at '{name}'.", this);
			return;
		}
		
		foreach (DoubleClip clip in Clips)
			clip.Initialize(Sequencer);
		
		Track.Initialize(Sequencer.Speed, Clips);
	}

	public override void Sample(float _MinTime, float _MaxTime)
	{
		base.Sample(_MinTime, _MaxTime);
		
		if (Track != null)
			Track.Process(_MaxTime);
	}

	#if UNITY_EDITOR
	public override void Sort()
	{
		base.Sort();
		
		if (Track != null)
			Track.Process();
	}
	#endif
}