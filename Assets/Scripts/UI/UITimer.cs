using System;
using TMPro;
using UnityEngine;

public class UITimer : UIEntity
{
	public enum TimerMode
	{
		BeforeStart,
		BeforeEnd,
		AfterStart,
		AfterEnd,
	}

	public TimerMode Mode
	{
		get => m_Mode;
		set
		{
			if (m_Mode == value)
				return;
			
			m_Mode = value;
			m_Time = 0;
			
			ProcessTimer();
		}
	}

	[SerializeField] TMP_Text  m_Timer;
	[SerializeField] TimerMode m_Mode;

	long   m_StartTimestamp;
	long   m_EndTimestamp;
	Action m_Callback;

	DateTime m_StartTime;
	DateTime m_EndTime;
	float    m_Time;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessTimer();
	}

	public void Setup(long _StartTimestamp, long _EndTimestamp, Action _Callback = null)
	{
		m_StartTimestamp = _StartTimestamp;
		m_EndTimestamp   = _EndTimestamp;
		m_StartTime      = TimeUtility.GetLocalTime(_StartTimestamp);
		m_EndTime        = TimeUtility.GetLocalTime(_EndTimestamp);
		m_Callback       = _Callback;
		m_Time           = 0;
	}

	public void Process() => ProcessTimer();

	void Update()
	{
		ProcessTimer();
	}

	void ProcessTimer()
	{
		if (Time.realtimeSinceStartup < m_Time)
			return;
		
		m_Time = Time.realtimeSinceStartup + 1;
		
		long timestamp = TimeUtility.GetTimestamp();
		
		switch (m_Mode)
		{
			case TimerMode.BeforeStart:
				ProcessBeforeStartTimer();
				if (m_Callback != null && timestamp >= m_StartTimestamp)
					InvokeCallback();
				break;
			case TimerMode.BeforeEnd:
				ProcessBeforeEndTimer();
				if (m_Callback != null && timestamp >= m_EndTimestamp)
					InvokeCallback();
				break;
			case TimerMode.AfterStart:
				ProcessAfterStartTimer();
				if (m_Callback != null && timestamp >= m_StartTimestamp)
					InvokeCallback();
				break;
			case TimerMode.AfterEnd:
				ProcessAfterEndTimer();
				if (m_Callback != null && timestamp >= m_EndTimestamp)
					InvokeCallback();
				break;
		}
	}

	void ProcessBeforeStartTimer() => ProcessTimer(m_StartTime - DateTime.Now);

	void ProcessBeforeEndTimer() => ProcessTimer(m_EndTime - DateTime.Now);

	void ProcessAfterStartTimer() => ProcessTimer(DateTime.Now - m_StartTime);

	void ProcessAfterEndTimer() => ProcessTimer(DateTime.Now - m_EndTime);

	void ProcessTimer(TimeSpan _Timer)
	{
		int hours   = Mathf.Max(0, (int)_Timer.TotalHours);
		int minutes = Mathf.Max(0, _Timer.Minutes);
		int seconds = Mathf.Max(0, _Timer.Seconds);
		
		m_Timer.text = $"{hours:00}:{minutes:00}:{seconds:00}";
	}

	void InvokeCallback()
	{
		Action action = m_Callback;
		m_Callback = null;
		action?.Invoke();
	}
}