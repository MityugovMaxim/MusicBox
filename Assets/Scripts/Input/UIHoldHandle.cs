using System;
using UnityEngine;
using UnityEngine.Events;

public class UIHoldHandle : UIHandle
{
	const float FRAME_ERROR = 20;

	[Serializable]
	public class HoldEvent : UnityEvent<bool> { }

	public float MinProgress { get; private set; }

	public float MaxProgress { get; private set; }

	protected override bool Processed => m_Processed;

	[SerializeField] UIHoldIndicator m_Indicator;
	[SerializeField] RectTransform   m_Marker;
	[SerializeField] RectOffset      m_Margin;

	bool m_Processed;
	bool m_Hold;
	bool m_Miss;
	Rect m_Area;

	public override void EnterZone()
	{
		if (m_Processed)
			return;
		
		MinProgress = 0;
		MaxProgress = 0;
		
		m_Hold = false;
		m_Miss = false;
	}

	public override void ExitZone()
	{
		if (m_Processed)
			return;
		
		ProcessFail();
		
		m_Processed = true;
		m_Hold      = false;
		m_Miss      = false;
	}

	public override void Reverse()
	{
		if (m_Processed)
			return;
		
		MinProgress = 0;
		MaxProgress = 0;
		
		m_Hold = false;
		m_Miss = false;
	}

	public override void Restore()
	{
		if (m_Marker != null)
			m_Marker.anchoredPosition = Vector2.zero;
		
		MinProgress = 0;
		MaxProgress = 0;
		
		m_Processed = false;
		m_Hold      = false;
		m_Miss      = false;
	}

	public void Process(float _Phase)
	{
		if (m_Processed)
			return;
		
		if (m_Hold)
		{
			MaxProgress = _Phase;
			
			if (MaxProgress >= 1 || Mathf.Approximately(MaxProgress, 1))
			{
				m_Hold      = false;
				m_Processed = true;
				
				ProcessSuccess();
			}
		}
		else
		{
			MinProgress = _Phase;
			MaxProgress = _Phase;
			
			Rect  indicatorRect = m_Indicator.GetWorldRect();
			Rect  handleRect    = GetWorldRect();
			float missThreshold = indicatorRect.yMin + handleRect.height;
			
			if (m_Miss || handleRect.yMin < missThreshold)
				return;
			
			m_Miss = true;
			
			ProcessMiss();
		}
	}

	public override void TouchDown(int _ID, Rect _Area)
	{
		if (m_Processed || m_Hold)
			return;
		
		m_Hold = true;
		m_Area = GetLocalRect(_Area);
		
		Rect rect = GetLocalRect(m_Indicator.GetWorldRect());
		
		float distance = Mathf.Abs(m_Area.yMin - rect.yMin);
		float length   = m_Area.height;
		float progress = 1.0f - Mathf.Max(0, distance - FRAME_ERROR) / length;
		
		ProcessHit(progress);
	}

	public override void TouchUp(int _ID, Rect _Area)
	{
		if (m_Processed || !m_Hold)
			return;
		
		if (m_Marker != null)
			m_Marker.anchoredPosition = Vector2.zero;
		
		m_Hold      = false;
		m_Processed = true;
		
		ProcessSuccess();
	}

	public override void TouchMove(int _ID, Rect _Area)
	{
		if (m_Processed || !m_Hold)
			return;
		
		Rect rect = GetLocalRect(m_Margin);
		Rect area = GetLocalRect(_Area);
		
		if (m_Marker != null)
			m_Marker.anchoredPosition = new Vector2(area.center.x - m_Area.center.x, 0);
		
		if (rect.Overlaps(area))
			return;
		
		m_Hold      = false;
		m_Processed = true;
		
		ProcessSuccess();
	}

	void ProcessHit(float _Progress)
	{
		m_Indicator.Hit(_Progress);
	}

	void ProcessMiss()
	{
		m_Indicator.Miss(MinProgress, MaxProgress);
	}

	void ProcessSuccess()
	{
		m_Indicator.Success(MinProgress, MaxProgress);
	}

	void ProcessFail()
	{
		m_Indicator.Fail(MinProgress, MaxProgress);
	}
}