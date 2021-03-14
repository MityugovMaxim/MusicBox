using UnityEngine;

public abstract class InputReader : MonoBehaviour, IRoutineClipReceiver
{
	protected float Time { get; private set; }
	protected Range SuccessRange => m_SuccessRange;
	protected Range PerfectRange => m_PerfectRange;

	[SerializeField] Range m_SuccessRange = new Range(0.5f, 1);
	[SerializeField] Range m_PerfectRange = new Range(0.8f, 0.9f);

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