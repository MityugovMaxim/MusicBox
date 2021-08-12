using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
public partial class BeatTrack
{
	protected override float MinHeight => 50;
	protected override float MaxHeight => 50;

	public override void Sort() { }
}
#endif

[CreateAssetMenu(fileName = "Beat Track", menuName = "Tracks/Beat Track")]
public partial class BeatTrack : Track
{
	UIBeatTrack Track { get; set; }

	[SerializeField, Reference(typeof(UIBeatTrack))] string m_Track;
	[SerializeField]                                 float  m_Frequency = 1;

	float m_BeatThreshold;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		Track = GetReference<UIBeatTrack>(m_Track);
		
		m_BeatThreshold = 60.0f * m_Frequency / Sequencer.BPM;
	}

	public override void Sample(float _MinTime, float _MaxTime)
	{
		float threshold = Mathf.Floor(_MaxTime / m_BeatThreshold) * m_BeatThreshold;
		
		if (_MinTime < threshold && _MaxTime >= threshold)
			Track.Beat(m_BeatThreshold * 0.5f);
	}

	public override IEnumerator<Clip> GetEnumerator()
	{
		return Enumerable.Empty<Clip>().GetEnumerator();
	}
}