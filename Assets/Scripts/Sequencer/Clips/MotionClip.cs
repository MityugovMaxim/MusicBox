using UnityEngine;

public class MotionClip : Clip
{
	[SerializeField] int m_StateHash;

	Animator m_Animator;

	public void Initialize(Sequencer _Sequencer, Animator _Animator)
	{
		base.Initialize(_Sequencer);
		
		m_Animator = _Animator;
	}

	protected override void OnEnter(float _Time)
	{
		m_Animator.WriteDefaultValues();
	}

	protected override void OnUpdate(float _Time)
	{
		m_Animator.speed = 0;
		m_Animator.Play(m_StateHash, 0, GetNormalizedTime(_Time));
		m_Animator.Update(0);
	}

	protected override void OnExit(float _Time)
	{
		m_Animator.speed = 0;
		m_Animator.Play("Default", 0, _Time);
		m_Animator.Update(0);
	}
}