using System.Collections;
using UnityEngine;

public class UIFlare : UIEntity
{
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");

	Animator    m_Animator;
	IEnumerator m_Routine;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_Animator.SetTrigger(m_RestoreParameterID);
		m_Animator.ResetTrigger(m_PlayParameterID);
		m_Animator.Update(0);
	}

	public void Play(float _Delay = 0)
	{
		if (m_Routine != null)
			StopCoroutine(m_Routine);
		
		m_Routine = null;
		
		if (_Delay < float.Epsilon)
		{
			m_Animator.SetTrigger(m_PlayParameterID);
			return;
		}
		
		m_Routine = PlayRoutine(_Delay);
		
		StartCoroutine(m_Routine);
	}

	IEnumerator PlayRoutine(float _Delay)
	{
		yield return new WaitForSeconds(_Delay);
		
		m_Animator.SetTrigger(m_PlayParameterID);
	}
}
