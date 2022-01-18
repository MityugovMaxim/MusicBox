using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public abstract class UITrack<T> : UIEntity where T : Clip
{
	protected float Time => m_Time;

	[SerializeField] float m_Time;
	[SerializeField] float m_Speed;
	[SerializeField] float m_MinPadding;
	[SerializeField] float m_MaxPadding;

	SignalBus   m_SignalBus;
	UIInputZone m_InputZone;

	[NonSerialized] List<T> m_Clips       = new List<T>();
	[NonSerialized] List<T> m_ClipsBuffer = new List<T>();

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		if (m_SignalBus != null)
			m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
	}

	[Inject]
	public void Construct(SignalBus _SignalBus, UIInputZone _InputZone)
	{
		m_InputZone = _InputZone;
		
		m_SignalBus = _SignalBus;
		if (m_SignalBus != null)
			m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
	}

	public void Initialize(float _Speed, List<T> _Clips)
	{
		m_Speed = _Speed;
		m_Clips = _Clips;
	}

	void RegisterLevelRestart()
	{
		Process();
	}

	protected float GetTime(float _Distance)
	{
		return _Distance / m_Speed + m_Time;
	}

	protected float GetDistance(float _Time)
	{
		return (_Time - m_Time) * m_Speed;
	}

	public void Process()
	{
		Process(m_Time);
	}

	public void Process(float _Time)
	{
		m_Time = _Time;
		
		float minTime = GetMinTime();
		float maxTime = GetMaxTime();
		foreach (T indicatorData in m_ClipsBuffer)
		{
			if (indicatorData.MinTime > maxTime || indicatorData.MaxTime < minTime)
				RemoveIndicator(indicatorData);
		}
		
		m_ClipsBuffer.Clear();
		
		int indicatorIndex = FindIndicatorIndex();
		
		if (indicatorIndex >= 0)
		{
			int minIndicatorIndex = FindMinIndicatorIndex(indicatorIndex);
			int maxIndicatorIndex = FindMaxIndicatorIndex(indicatorIndex);
			
			for (int i = minIndicatorIndex; i <= maxIndicatorIndex; i++)
				m_ClipsBuffer.Add(m_Clips[i]);
		}
		
		DrawIndicators(m_ClipsBuffer);
	}

	protected abstract void RemoveIndicator(T _Clip);

	protected abstract void DrawIndicators(List<T> _Clips);

	int FindIndicatorIndex()
	{
		float minTime = GetMinTime();
		float maxTime = GetMaxTime();
		
		int i = 0;
		int j = m_Clips.Count - 1;
		while (i <= j)
		{
			int index = (i + j) / 2;
			
			T indicatorData = m_Clips[index];
			
			if (indicatorData.MinTime > maxTime)
				j = index - 1;
			else if (indicatorData.MaxTime < minTime)
				i = index + 1;
			else
				return index;
		}
		
		return -1;
	}

	int FindMinIndicatorIndex(int _Index)
	{
		float minTime = GetMinTime();
		float maxTime = GetMaxTime();
		
		int index = _Index;
		
		for (int i = _Index - 1; i >= 0; i--)
		{
			T indicatorData = m_Clips[i];
			
			if (indicatorData.MinTime > maxTime || indicatorData.MaxTime < minTime)
				break;
			
			index = i;
		}
		
		return index;
	}

	int FindMaxIndicatorIndex(int _Index)
	{
		float minTime = GetMinTime();
		float maxTime = GetMaxTime();
		
		int index = _Index;
		
		for (int i = _Index + 1; i < m_Clips.Count; i++)
		{
			T indicatorData = m_Clips[i];
			
			if (indicatorData.MinTime > maxTime || indicatorData.MaxTime < minTime)
				break;
			
			index = i;
		}
		
		return index;
	}

	float GetMinTime()
	{
		Rect rect = RectTransform.GetLocalRect(m_InputZone.RectTransform);
		
		return GetTime(rect.yMin - m_MinPadding);
	}

	float GetMaxTime()
	{
		Rect rect = RectTransform.GetLocalRect(m_InputZone.RectTransform);
		
		return GetTime(rect.yMax + m_MaxPadding);
	}

	protected Vector2 GetAnchor()
	{
		return new Vector2(
			0.5f,
			RectTransform.GetVerticalAnchor(m_InputZone.RectTransform, 0.5f)
		);
	}
}