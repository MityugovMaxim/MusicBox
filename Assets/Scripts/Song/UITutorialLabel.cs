using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UITutorialLabel : UIOrder
{
	public override int Thickness => 1;

	static readonly int m_ShowParameterID = Animator.StringToHash("Show");

	Animator m_Animator;
	bool     m_Shown;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	public void Show()
	{
		if (m_Shown)
			return;
		
		m_Shown = true;
		
		m_Animator.SetBool(m_ShowParameterID, true);
	}

	public void Hide()
	{
		if (!m_Shown)
			return;
		
		m_Shown = false;
		
		m_Animator.SetBool(m_ShowParameterID, false);
	}
}