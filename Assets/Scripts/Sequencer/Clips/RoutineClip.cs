using UnityEngine;

public interface IRoutineClipReceiver
{
	void StartRoutine(float  _Time);
	void UpdateRoutine(float _Time);
	void FinishRoutine(float _Time);
}

public class RoutineClip : Clip
{
	IRoutineClipReceiver[] m_Receivers;

	public void Initialize(IRoutineClipReceiver[] _Receivers)
	{
		m_Receivers = _Receivers;
	}

	protected override void OnEnter(float _Time)
	{
		float time = GetNormalizedTime(_Time);
		foreach (IRoutineClipReceiver receiver in m_Receivers)
			receiver.StartRoutine(time);
	}

	protected override void OnUpdate(float _Time)
	{
		if (!Application.isPlaying)
			return;
		
		float time = GetNormalizedTime(_Time);
		foreach (IRoutineClipReceiver receiver in m_Receivers)
			receiver.UpdateRoutine(time);
	}

	protected override void OnExit(float _Time)
	{
		if (!Application.isPlaying)
			return;
		
		float time = GetNormalizedTime(_Time);
		foreach (IRoutineClipReceiver receiver in m_Receivers)
			receiver.FinishRoutine(time);
	}

	protected override void OnStop(float _Time)
	{
		if (!Application.isPlaying)
			return;
		
		float time = GetNormalizedTime(_Time);
		foreach (IRoutineClipReceiver receiver in m_Receivers)
			receiver.FinishRoutine(time);
	}
}