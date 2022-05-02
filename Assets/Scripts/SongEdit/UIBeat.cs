using System;
using System.Collections.Generic;
using AudioBox.ASF;
using UnityEngine;
using Zenject;

public class UIBeat : UIEntity
{
	const double BPM_STEP = 4d;

	public double Time
	{
		get => m_Time;
		set
		{
			if (Math.Abs(m_Time - value) < double.Epsilon * 2)
				return;
			
			m_Time = value;
			
			ProcessTime();
		}
	}

	public double Origin
	{
		get => m_Origin;
		set
		{
			if (Math.Abs(m_Origin - value) < double.Epsilon * 2)
				return;
			
			m_Origin = value;
			
			ProcessTime();
		}
	}

	public double Step => 60d / BPM / BPM_STEP;

	public float BPM
	{
		get => m_BPM;
		set
		{
			if (Mathf.Approximately(m_BPM, value))
				return;
			
			m_BPM = value;
			
			ProcessTime();
		}
	}

	public float Duration
	{
		get => m_Duration;
		set
		{
			if (Mathf.Approximately(m_Duration, value))
				return;
			
			m_Duration = value;
			
			ProcessLimits();
			ProcessTime();
		}
	}

	public float Ratio
	{
		get => m_Ratio;
		set
		{
			if (Mathf.Approximately(m_Ratio, value))
				return;
			
			m_Ratio = value;
			
			ProcessLimits();
			ProcessTime();
		}
	}

	public bool SnapActive { get; set; }

	public double MinTime { get; private set; }

	public double MaxTime { get; private set; }

	[SerializeField] double m_Origin;
	[SerializeField] double m_Time;
	[SerializeField] float  m_Duration;
	[SerializeField] float  m_Ratio;
	[SerializeField] float  m_BPM;

	[Inject] UIBeatKey.Pool m_ItemPool;

	readonly List<UIBeatKey> m_Items     = new List<UIBeatKey>();
	readonly List<float>     m_Keys      = new List<float>();
	readonly List<double>    m_Intervals = new List<double>();

	protected override void Awake()
	{
		base.Awake();
		
		ProcessLimits();
		ProcessTime();
	}

	public double Snap(double _Time)
	{
		if (!SnapActive)
			return _Time;
		
		double step = Step;
		
		double time = Origin + _Time;
		
		return Math.Round(time / step) * step - Origin;
	}

	void ProcessTime()
	{
		ProcessKeys();
		
		int delta = m_Keys.Count - m_Items.Count;
		int count = Mathf.Abs(delta);
		if (delta > 0)
		{
			for (int i = 0; i < count; i++)
				m_Items.Add(m_ItemPool.Spawn(RectTransform));
		}
		else
		{
			for (int i = 0; i < count; i++)
				m_ItemPool.Despawn(m_Items[i]);
			m_Items.RemoveRange(0, count);
		}
		
		for (int i = 0; i < m_Items.Count; i++)
		{
			m_Items[i].Position = m_Keys[i];
			m_Items[i].Time     = m_Intervals[i];
		}
	}

	void ProcessKeys()
	{
		m_Keys.Clear();
		
		if (Math.Abs(MaxTime - MinTime) < double.Epsilon * 2)
			return;
		
		Rect rect = GetLocalRect();
		
		double minTime = Origin + Time + MinTime;
		double maxTime = Origin + Time + MaxTime;
		
		double step = 60d / m_BPM / BPM_STEP;
		
		double time = Math.Ceiling(minTime / step) * step - step;
		while (time <= maxTime + step)
		{
			float position = ASFMath.TimeToPosition(time, minTime, maxTime, rect.yMin, rect.yMax);
			
			m_Keys.Add(position);
			m_Intervals.Add(Origin + time);
			
			time += step;
		}
		
		m_Keys.Reverse();
		m_Intervals.Reverse();
	}

	void ProcessLimits()
	{
		MinTime = Duration * (Ratio - 1);
		MaxTime = Duration * Ratio;
	}
}