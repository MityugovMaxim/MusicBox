using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if UNITY_EDITOR
public partial class MotionTrack
{
	protected override float MinHeight => 50;
	protected override float MaxHeight => 50;

	public override void DropPerform(float _Time, Object[] _Objects)
	{
		Animator animator = GetReference<Animator>(m_Animator);
		
		if (animator == null)
			return;
		
		AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
		
		if (controller == null)
			return;
		
		AnimationClip[] animationClips = _Objects.OfType<AnimationClip>().ToArray();
		
		foreach (AnimationClip animationClip in animationClips)
		{
			AnimatorState state = FindState(controller, animationClip);
			
			if (state == null)
				state = controller.AddMotion(animationClip, 0);
			
			if (state == null)
				continue;
			
			MotionClip motionClip = CreateInstance<MotionClip>();
			
			motionClip.name = "Motion Clip";
			
			using (SerializedObject motionClipObject = new SerializedObject(motionClip))
			{
				SerializedProperty stateHashProperty = motionClipObject.FindProperty("m_StateHash");
				
				stateHashProperty.intValue = state.nameHash;
				
				motionClipObject.ApplyModifiedProperties();
			}
			
			float duration = !Mathf.Approximately(animationClip.length, 0) ? animationClip.length : 1;
			
			TrackUtility.AddClip(this, motionClip, _Time, duration);
			
			_Time += animationClip.length;
			
			motionClip.Initialize(Sequencer, animator);
		}
	}

	static AnimatorState FindState(AnimatorController _Controller, AnimationClip _AnimationClip)
	{
		foreach (AnimatorControllerLayer layer in _Controller.layers)
		{
			AnimatorState state = FindState(layer.stateMachine, _AnimationClip);
			
			if (state != null)
				return state;
		}
		return null;
	}

	static AnimatorState FindState(AnimatorStateMachine _StateMachine, AnimationClip _AnimationClip)
	{
		foreach (ChildAnimatorState childState in _StateMachine.states)
		{
			AnimatorState state = childState.state;
			
			if (state.motion == _AnimationClip)
				return state;
		}
		
		foreach (ChildAnimatorStateMachine childStateMachine in _StateMachine.stateMachines)
		{
			AnimatorStateMachine stateMachine = childStateMachine.stateMachine;
			
			AnimatorState state = FindState(stateMachine, _AnimationClip);
			
			if (state != null)
				return state;
		}
		
		return null;
	}
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
		
		animator.WriteDefaultValues();
		
		foreach (MotionClip clip in Clips)
			clip.Initialize(Sequencer, animator);
	}
}