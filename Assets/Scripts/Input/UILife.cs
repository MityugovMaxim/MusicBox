using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UILife : UIEntity
{
	Animator Animator
	{
		get
		{
			if (m_Animator == null)
			{
				m_Animator = GetComponent<Animator>();
				
				m_Animator.keepAnimatorControllerStateOnDisable = true;
			}
			return m_Animator;
		}
	}

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_DisableParameterID = Animator.StringToHash("Disable");
	static readonly int m_EnableParameterID  = Animator.StringToHash("Enable");

	Animator m_Animator;

	bool m_Active;

	public void SetActive(bool _Value)
	{
		if (_Value)
			Enable();
		else
			Disable();
	}

	public void Enable()
	{
		if (m_Active)
			return;
		
		m_Active = true;
		
		Animator.ResetTrigger(m_DisableParameterID);
		Animator.SetTrigger(m_EnableParameterID);
	}

	public void Disable()
	{
		if (!m_Active)
			return;
		
		m_Active = false;
		
		Animator.ResetTrigger(m_EnableParameterID);
		Animator.SetTrigger(m_DisableParameterID);
	}

	public void Restore()
	{
		m_Active = true;
		
		Animator.ResetTrigger(m_EnableParameterID);
		Animator.ResetTrigger(m_DisableParameterID);
		Animator.SetTrigger(m_RestoreParameterID);
	}
}