using UnityEngine;

public interface IRoutineClipReceiver
{
	void StartRoutine(RoutineClipData _Data);
	void UpdateRoutine(RoutineClipData _Data);
	void FinishRoutine(RoutineClipData _Data);
}

public struct RoutineClipData
{
	public float Time     { get; }
	public float Duration { get; }

	public RoutineClipData(float _Time, float _Duration)
	{
		Time     = _Time;
		Duration = _Duration;
	}
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
		RoutineClipData data = new RoutineClipData(
			GetLocalTime(_Time),
			MaxTime - MinTime
		);
		
		foreach (IRoutineClipReceiver receiver in m_Receivers)
			receiver.StartRoutine(data);
	}

	protected override void OnUpdate(float _Time)
	{
		if (!Application.isPlaying)
			return;
		
		RoutineClipData data = new RoutineClipData(
			GetLocalTime(_Time),
			MaxTime - MinTime
		);
		
		foreach (IRoutineClipReceiver receiver in m_Receivers)
			receiver.UpdateRoutine(data);
	}

	protected override void OnExit(float _Time)
	{
		if (!Application.isPlaying)
			return;
		
		RoutineClipData data = new RoutineClipData(
			GetLocalTime(_Time),
			MaxTime - MinTime
		);
		
		foreach (IRoutineClipReceiver receiver in m_Receivers)
			receiver.FinishRoutine(data);
	}

	protected override void OnStop(float _Time)
	{
		if (!Application.isPlaying)
			return;
		
		RoutineClipData data = new RoutineClipData(
			GetLocalTime(_Time),
			MaxTime - MinTime
		);
		
		foreach (IRoutineClipReceiver receiver in m_Receivers)
			receiver.FinishRoutine(data);
	}
}