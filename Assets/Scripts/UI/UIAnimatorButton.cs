using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UIAnimatorButton : UIButton
{
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_NormalParameterID  = Animator.StringToHash("Normal");
	static readonly int m_PressParameterID   = Animator.StringToHash("Press");
	static readonly int m_ClickParameterID   = Animator.StringToHash("Click");

	Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_Animator.ResetTrigger(m_NormalParameterID);
		m_Animator.ResetTrigger(m_PressParameterID);
		m_Animator.ResetTrigger(m_ClickParameterID);
		m_Animator.SetTrigger(m_RestoreParameterID);
		
		m_Animator.Update(0);
	}

	protected override void OnNormal()
	{
		m_Animator.SetTrigger(m_NormalParameterID);
	}

	protected override void OnPress()
	{
		m_Animator.SetTrigger(m_PressParameterID);
	}

	protected override void OnClick()
	{
		m_Animator.SetTrigger(m_ClickParameterID);
	}
}
