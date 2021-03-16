using UnityEngine;

#if UNITY_EDITOR
public partial class InputClip
{
	public void Setup(float _Duration, float _Time, float _MinZone, float _MaxZone)
	{
		float sourceTime = MathUtility.Remap(m_ZoneTime, 0, 1, MinTime, MaxTime);
		float targetTime = MathUtility.Remap(_Time, 0, 1, MinTime, MaxTime);
		float deltaTime  = targetTime - sourceTime;
		float minTime    = targetTime - _Duration * _Time;
		float maxTime    = targetTime + _Duration * (1 - _Time);
		
		MinTime = minTime - deltaTime;
		MaxTime = maxTime - deltaTime;
		
		m_ZoneTime = _Time;
		m_MinZone  = _MinZone;
		m_MaxZone  = _MaxZone;
	}
}
#endif

public partial class InputClip : Clip
{
	public float ZoneTime => m_ZoneTime;
	public float MinZone  => m_MinZone;
	public float MaxZone  => m_MaxZone;

	int         m_ID;
	InputReader m_InputReader;
	float       m_ZoneTime;
	float       m_MinZone;
	float       m_MaxZone;
	bool        m_Reading;

	public void Initialize(int _ID, InputReader _InputReader, float _Time, float _MinZone, float _MaxZone)
	{
		m_ID          = _ID;
		m_InputReader = _InputReader;
		m_ZoneTime    = _Time;
		m_MinZone     = _MinZone;
		m_MaxZone     = _MaxZone;
	}

	protected override void OnEnter(float _Time)
	{
		if (!Application.isPlaying)
			return;
		
		float time = GetNormalizedTime(_Time);
		
		m_InputReader.StartProcessing(m_ID, time);
	}

	protected override void OnUpdate(float _Time)
	{
		if (!Application.isPlaying)
			return;
		
		float time = GetNormalizedTime(_Time);
		
		m_InputReader.UpdateProcessing(m_ID, time);
		
		if (time >= m_MinZone && !m_Reading)
		{
			m_Reading = true;
			m_InputReader.StartInput(m_ID);
		}
		
		if (time >= m_MaxZone && m_Reading)
		{
			m_Reading = false;
			m_InputReader.FinishInput(m_ID);
		}
	}

	protected override void OnExit(float _Time)
	{
		if (!Application.isPlaying)
			return;
		
		float time = GetNormalizedTime(_Time);
		
		if (m_Reading)
		{
			m_Reading = false;
			m_InputReader.FinishInput(m_ID);
		}
		
		m_InputReader.FinishProcessing(m_ID, time);
	}

	protected override void OnStop(float _Time)
	{
		if (!Application.isPlaying)
			return;
		
		float time = GetNormalizedTime(_Time);
		
		m_Reading = false;
		
		m_InputReader.FinishProcessing(m_ID, time);
	}
}