using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UITapIndicator : UIIndicator
{
	public override UIHandle Handle => m_Handle;
	public override float MinPadding => RectTransform.rect.yMin;
	public override float MaxPadding => RectTransform.rect.yMax;

	Animator Animator
	{
		get
		{
			if (m_Animator == null)
				m_Animator = GetComponent<Animator>();
			return m_Animator;
		}
	}

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_SuccessParameterID = Animator.StringToHash("Success");
	static readonly int m_FailParameterID    = Animator.StringToHash("Fail");

	[SerializeField] UITapHandle m_Handle;

	Animator m_Animator;

	public void Setup()
	{
		if (m_Handle != null)
		{
			m_Handle.OnSuccess += Success;
			m_Handle.OnFail    += Fail;
		}
	}

	public void Restore()
	{
		if (m_Handle != null)
		{
			m_Handle.StopReceiveInput();
			
			m_Handle.OnSuccess -= Success;
			m_Handle.OnFail    -= Fail;
		}
		
		Animator.ResetTrigger(m_SuccessParameterID);
		Animator.ResetTrigger(m_FailParameterID);
		Animator.SetTrigger(m_RestoreParameterID);
		
		if (gameObject.activeInHierarchy)
			Animator.Update(0);
	}

	void Success(float _Progress)
	{
		Animator.SetTrigger(m_SuccessParameterID);
	}

	void Fail(float _Progress)
	{
		Animator.SetTrigger(m_FailParameterID);
	}
}