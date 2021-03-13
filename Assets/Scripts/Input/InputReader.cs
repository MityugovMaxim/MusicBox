using UnityEngine;

public abstract class InputReader : MonoBehaviour, IRoutineClipReceiver
{
	protected float Time { get; private set; }
	protected float SuccessTime => m_SuccessTime;
	protected float PerfectTime => m_PerfectTime;

	[SerializeField] float m_SuccessTime;
	[SerializeField] float m_PerfectTime;

	bool m_Reading;

	public virtual void StartRoutine(float _Time)
	{
		m_Reading = true;
		
		Time = _Time;
	}

	public virtual void UpdateRoutine(float _Time)
	{
		Time = _Time;
	}

	public virtual void FinishRoutine(float _Time)
	{
		if (m_Reading)
			Fail();
		
		Time = _Time;
		
		m_Reading = false;
	}

	public void ProcessInput()
	{
		if (m_Reading)
			Success();
		else
			Fail();
		
		m_Reading = false;
	}

	protected abstract void Success();

	protected abstract void Fail();
}