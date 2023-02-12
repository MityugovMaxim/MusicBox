using System;
using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class UIAnalogTimer : UIEntity
{
	string ID => GetInstanceID().ToString();

	[SerializeField] RectTransform m_Content;
	[SerializeField] UIGroup       m_DaysGroup;
	[SerializeField] UIGroup       m_TimeGroup;
	[SerializeField] UIAnalogDigit m_Day1;
	[SerializeField] UIAnalogDigit m_Day2;
	[SerializeField] UIAnalogDigit m_Hour1;
	[SerializeField] UIAnalogDigit m_Hour2;
	[SerializeField] UIAnalogDigit m_Minute1;
	[SerializeField] UIAnalogDigit m_Minute2;
	[SerializeField] UIAnalogDigit m_Second1;
	[SerializeField] UIAnalogDigit m_Second2;

	long m_StartTimestamp;
	long m_EndTimestamp;

	IEnumerator m_TickRoutine;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		#if UNITY_EDITOR
		if (!IsInstanced)
			return;
		#endif
		
		ProcessSize();
		
		ProcessTimer(true);
		
		TickTimer();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		TokenProvider.CancelToken(this, ID);
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		
		ProcessSize();
	}

	public void SetTime(long _Time)
	{
		long timestamp = TimeUtility.GetTimestamp();
		
		SetTime(timestamp, timestamp + _Time);
	}

	public void SetTime(long _StartTimestamp, long _EndTimestamp)
	{
		if (m_TickRoutine != null)
			StopCoroutine(m_TickRoutine);
		m_TickRoutine = null;
		
		m_StartTimestamp = _StartTimestamp;
		m_EndTimestamp   = _EndTimestamp;
		
		ProcessTimer(true);
	}

	public void SetTimer(long _Timestamp)
	{
		m_StartTimestamp = TimeUtility.GetTimestamp();
		m_EndTimestamp   = _Timestamp;
		
		ProcessTimer(true);
		
		if (gameObject.activeInHierarchy)
			TickTimer();
	}

	public void SetTimer(long _StartTimestamp, long _EndTimestamp)
	{
		m_StartTimestamp = _StartTimestamp;
		m_EndTimestamp   = _EndTimestamp;
		
		ProcessTimer(true);
		
		if (gameObject.activeInHierarchy)
			TickTimer();
	}

	void TickTimer()
	{
		if (m_TickRoutine != null)
			StopCoroutine(m_TickRoutine);
		
		m_TickRoutine = TickRoutine();
		
		StartCoroutine(m_TickRoutine);
	}

	IEnumerator TickRoutine()
	{
		long timestamp = TimeUtility.GetTimestamp();
		
		while (timestamp <= m_EndTimestamp)
		{
			float delay = (1000 - timestamp % 1000) * 0.001f;
			
			yield return new WaitForSeconds(delay);
			
			ProcessTimer();
			
			timestamp = TimeUtility.GetTimestamp();
		}
		
		ProcessTimer(true);
	}

	void ProcessTimer(bool _Instant = false)
	{
		long delta = m_EndTimestamp - m_StartTimestamp;
		
		long timer;
		if (delta > 0)
			timer = delta - MathUtility.RemapClamped(TimeUtility.GetTimestamp(), m_StartTimestamp, m_EndTimestamp, 0, delta);
		else
			timer = 0;
		
		TimeSpan time = TimeSpan.FromMilliseconds(timer);
		
		int days    = (int)time.TotalDays;
		int hours   = (int)time.TotalHours;
		int minutes = time.Minutes;
		int seconds = time.Seconds;
		
		if (days >= 3)
		{
			m_DaysGroup.Show(_Instant);
			m_TimeGroup.Hide(_Instant);
		}
		else
		{
			m_TimeGroup.Show(_Instant);
			m_DaysGroup.Hide(_Instant);
		}
		
		m_Day1.SetValue(days % 10, _Instant);
		m_Day2.SetValue(days / 10 % 10, _Instant);
		m_Hour1.SetValue(hours % 10, _Instant);
		m_Hour2.SetValue(hours / 10 % 10, _Instant);
		m_Minute1.SetValue(minutes % 10, _Instant);
		m_Minute2.SetValue(minutes / 10 % 10, _Instant);
		m_Second1.SetValue(seconds % 10, _Instant);
		m_Second2.SetValue(seconds / 10 % 10, _Instant);
	}

	void ProcessSize()
	{
		Vector2 size = m_Content.rect.size;
		
		Rect rect = GetLocalRect();
		
		float scale = Mathf.Min(
			rect.width / size.x,
			rect.height / size.y
		);
		
		m_Content.localScale = new Vector3(scale, scale, 1);
		
		m_Content.ForceUpdateRectTransforms();
	}
}
