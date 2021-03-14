using System.Collections;
using UnityEngine;

public abstract class InputReader : MonoBehaviour, IRoutineClipReceiver
{
	[SerializeField] float m_Complete;
	[SerializeField] float m_Expand;
	[SerializeField] float m_MinRange;
	[SerializeField] float m_MaxRange;

	float m_Time;
	bool  m_Reading;
	bool  m_Processed;

	// Enter(Time)
	// Process(Time, State) State : FAIL, BAD, GOOD, PERFECT
	// Exit(Time)

	public void StartRoutine(RoutineClipData _Data)
	{
		StartCoroutine(ProcessRoutine(_Data.Time, _Data.Duration + _Data.Duration * m_Expand));
	}

	public void UpdateRoutine(RoutineClipData _Data) { }

	public void FinishRoutine(RoutineClipData _Data) { }

	IEnumerator ProcessRoutine(float _Time, float _Duration)
	{
		Begin();
		
		m_Processed = false;
		
		bool process = false;
		
		float time = _Time;
		while (time < _Duration && !m_Processed)
		{
			float phase = time / _Duration;
			
			if (phase >= m_MinRange && !process)
			{
				process   = true;
				m_Reading = true;
			}
			
			if (process && !m_Reading)
				break;
			
			Process(phase);
			
			yield return null;
			
			time += Time.deltaTime;
		}
		
		m_Reading = false;
		
		if (!m_Processed)
			Fail();
		
		time = 0;
		while (time < m_Complete)
		{
			Complete(time / m_Complete);
			
			yield return null;
			
			time += Time.deltaTime;
		}
		
		Complete(1);
		
		Finish();
	}

	public void ProcessInput()
	{
		if (m_Reading)
			Success();
		else
			Fail();
		
		m_Reading = false;
		m_Processed = true;
	}

	protected abstract void Begin();
	protected abstract void Process(float _Time);
	protected abstract void Complete(float _Time);
	protected abstract void Finish();
	protected abstract void Success();
	protected abstract void Fail();
}