using UnityEngine;

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
	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		Haptic.Initialize();
	}
}