using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BeatView : MonoBehaviour
{
	Animator Animator
	{
		get
		{
			if (m_Animator == null)
				m_Animator = GetComponent<Animator>();
			return m_Animator;
		}
	}

	static readonly int m_SpeedParameterID = Animator.StringToHash("Speed");
	static readonly int m_PlayParameterID = Animator.StringToHash("Play");

	Animator m_Animator;

	public virtual void Play(float _Duration)
	{
		float speed = 1.0f / _Duration;
		
		Animator.SetFloat(m_SpeedParameterID, speed);
		Animator.SetTrigger(m_PlayParameterID);
	}
}