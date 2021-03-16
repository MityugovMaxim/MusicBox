using UnityEditor.Animations;
using UnityEngine;

public class MotionClip : Clip
{
	[SerializeField] AnimationClip m_AnimationClip;

	Animator      m_Animator;
	int           m_LayerIndex;
	AnimatorState m_State;

	public void Initialize(Sequencer _Sequencer, Animator _Animator, int _LayerIndex)
	{
		base.Initialize(_Sequencer);
		
		m_Animator   = _Animator;
		m_LayerIndex = _LayerIndex;
		
		m_Animator.WriteDefaultValues();
	}

	protected override void OnEnter(float _Time)
	{
		m_Animator.WriteDefaultValues();
	}

	protected override void OnUpdate(float _Time)
	{
		m_Animator.speed = 0;
		m_Animator.Play(m_AnimationClip.name, 0, GetNormalizedTime(_Time));
		m_Animator.Update(0);
	}

	protected override void OnExit(float _Time)
	{
		m_Animator.speed = 0;
		m_Animator.Play("Default", 0, _Time);
		m_Animator.Update(0);
	}
}