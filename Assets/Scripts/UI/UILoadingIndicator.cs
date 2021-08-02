using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UILoadingIndicator : UIEntity
{
	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	public void Play()
	{
		if (m_Animator != null)
			m_Animator.SetTrigger(m_PlayParameterID);
	}

	public void Restore()
	{
		if (m_Animator != null)
		{
			m_Animator.ResetTrigger(m_PlayParameterID);
			m_Animator.SetTrigger(m_RestoreParameterID);
		}
	}
}