using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UIPreviewControl : UIEntity
{
	static readonly int m_ShowParameterID    = Animator.StringToHash("Show");
	static readonly int m_InstantParameterID = Animator.StringToHash("Instant");

	bool     m_Shown;
	Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	public void Show(bool _Instant = false)
	{
		if (m_Shown)
			return;
		
		m_Shown = true;
		
		if (_Instant)
			m_Animator.SetTrigger(m_InstantParameterID);
		
		m_Animator.SetBool(m_ShowParameterID, true);
	}

	public void Hide(bool _Instant = false)
	{
		if (!m_Shown)
			return;
		
		m_Shown = false;
		
		if (_Instant)
			m_Animator.SetTrigger(m_InstantParameterID);
		
		m_Animator.SetBool(m_ShowParameterID, false);
	}
}