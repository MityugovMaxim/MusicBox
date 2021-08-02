using System;
using UnityEngine;

#if UNITY_EDITOR
public partial class TapTrack
{
	protected override float MinHeight => 50;
	protected override float MaxHeight => 50;
}
#endif

[CreateAssetMenu(fileName = "Tap Track", menuName = "Tracks/Tap Track")]
public partial class TapTrack : Track<TapClip>
{
	UITapTrack Track
	{
		get
		{
			if (m_TrackReference == null || m_TrackCache != m_Track)
			{
				m_TrackReference = GetReference<UITapTrack>(m_Track);
				m_TrackCache     = m_Track;
			}
			return m_TrackReference;
		}
	}

	[SerializeField, Reference(typeof(UITapTrack))] string m_Track;

	[NonSerialized] string     m_TrackCache;
	[NonSerialized] UITapTrack m_TrackReference;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		if (Track == null)
		{
			Debug.LogError($"[TapTrack] Track is not assigned at '{name}'.", this);
			return;
		}
		
		foreach (TapClip clip in Clips)
			clip.Initialize(Sequencer);
		
		Track.Initialize(Clips);
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