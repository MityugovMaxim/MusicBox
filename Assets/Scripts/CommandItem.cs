using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CommandItem : MonoBehaviour
{
	public CommandType Type => m_Type;

	Animator Animator
	{
		get
		{
			if (m_Animator == null)
				m_Animator = GetComponent<Animator>();
			return m_Animator;
		}
	}

	static readonly int m_SpeedParameterID   = Animator.StringToHash("Speed");
	static readonly int m_ShowParameterID    = Animator.StringToHash("Show");
	static readonly int m_SuccessParameterID = Animator.StringToHash("Success");
	static readonly int m_FailParameterID    = Animator.StringToHash("Fail");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField] CommandType m_Type;

	Animator m_Animator;

	public void Show(float _Duration)
	{
		float speed = 1.0f / _Duration;
		
		Animator.SetFloat(m_SpeedParameterID, speed);
		Animator.SetBool(m_ShowParameterID, true);
	}

	public void Hide(float _Duration)
	{
		float speed = 1.0f / _Duration;
		
		Animator.SetFloat(m_SpeedParameterID, speed);
		Animator.SetBool(m_ShowParameterID, false);
	}

	public void Success(float _Duration)
	{
		float speed = 1.0f / _Duration;
		
		Animator.SetFloat(m_SpeedParameterID, speed);
		Animator.SetTrigger(m_SuccessParameterID);
	}

	public void Fail(float _Duration)
	{
		float speed = 1.0f / _Duration;
		
		Animator.SetFloat(m_SpeedParameterID, speed);
		Animator.SetTrigger(m_FailParameterID);
	}

	void Remove()
	{
		Animator.SetFloat(m_SpeedParameterID, 1);
		Animator.SetBool(m_ShowParameterID, false);
		Animator.ResetTrigger(m_SuccessParameterID);
		Animator.ResetTrigger(m_FailParameterID);
		Animator.SetTrigger(m_RestoreParameterID);
		
		Destroy(gameObject);
	}
}