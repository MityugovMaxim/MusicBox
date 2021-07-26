using UnityEngine;
using Zenject;

#if UNITY_EDITOR
public partial class HapticTrack
{
	protected override float MinHeight => 30;
	protected override float MaxHeight => 30;
}
#endif

[CreateAssetMenu(fileName = "Haptic Track", menuName = "Tracks/Haptic Track")]
public partial class HapticTrack : Track<HapticClip>
{
	[Inject] Haptic m_Haptic;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		foreach (HapticClip clip in Clips)
			clip.Initialize(Sequencer, m_Haptic);
	}
}