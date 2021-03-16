using UnityEditor.Animations;
using UnityEngine;

#if UNITY_EDITOR
public partial class MotionTrack
{
	protected override float MinHeight => 50;
	protected override float MaxHeight => 50;
}
#endif

[CreateAssetMenu(fileName = "Motion Track", menuName = "Tracks/Motion Track")]
public partial class MotionTrack : Track<MotionClip>
{
	[SerializeField, Reference(typeof(Animator))] string m_Animator;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		Animator animator = GetReference<Animator>(m_Animator);
		
		AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
		
		controller.AddLayer("test_layer");
		
		int layerIndex = controller.layers.Length - 1;
		
		foreach (MotionClip clip in Clips)
			clip.Initialize(Sequencer, animator, layerIndex);
	}
}