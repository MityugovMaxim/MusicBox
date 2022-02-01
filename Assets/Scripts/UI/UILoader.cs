using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UILoader : UIEntity
{
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	public void Restore()
	{
		if (m_Animator != null)
			m_Animator.SetTrigger(m_RestoreParameterID);
	}
}