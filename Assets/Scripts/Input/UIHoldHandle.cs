using System;
using UnityEngine;
using UnityEngine.Events;

public class UIHoldHandle : UIHandle
{
	[Serializable]
	public class HoldEvent : UnityEvent<bool> { }

	float Progress { get; set; }
	float Length   => Mathf.Max(0, m_Max - m_Min);

	protected override bool Processed => m_Processed;

	[SerializeField] UIHoldIndicator m_Indicator;
	[SerializeField] RectTransform   m_Marker;
	[SerializeField] RectOffset      m_Margin;

	bool  m_Processed;
	bool  m_Hold;
	float m_Position;
	float m_Min;
	float m_Max;

	public override void EnterZone()
	{
		if (m_Processed)
			return;
		
		m_Min = 0;
		m_Max = 0;
	}

	public override void ExitZone()
	{
		if (m_Processed)
			return;
		
		m_Processed = true;
		
		if (m_Hold)
			m_Indicator.Success(Progress, Length);
		else
			m_Indicator.Fail();
		
		m_Hold = false;
	}

	public override void Reverse()
	{
		if (m_Processed)
			return;
		
		m_Min = 0;
		m_Max = 0;
	}

	public override void Restore()
	{
		if (m_Marker != null)
			m_Marker.anchoredPosition = Vector2.zero;
		
		m_Min = 0;
		m_Max = 0;
		
		m_Processed = false;
		m_Hold      = false;
	}

	public void Process(float _Phase)
	{
		if (m_Processed)
			return;
		
		m_Max = _Phase;
		
		if (m_Max < 1 && !Mathf.Approximately(m_Max, 1))
			return;
		
		m_Processed = true;
		
		if (m_Hold)
			m_Indicator.Success(Progress, Length);
		else
			m_Indicator.Fail();
		
		m_Hold = false;
	}

	public override void TouchDown(int _ID, Rect _Area)
	{
		if (m_Processed)
			return;
		
		m_Hold = true;
		
		m_Min = m_Max;
		
		Rect rect = RectTransform.rect;
		Rect area = GetLocalRect(_Area);
		
		Rect indicator = GetLocalRect(m_Indicator.GetWorldRect());
		
		m_Position = area.center.x;
		
		float position = area.center.y;
		float distance = Mathf.Abs(position - (indicator.yMin + rect.height * 0.5f));
		float length   = area.height;
		
		Progress = Mathf.Max(0, 1.0f - distance / length);
		
		m_Indicator.Hit();
	}

	public override void TouchMove(int _ID, Rect _Area)
	{
		if (m_Processed)
			return;
		
		Rect area = GetLocalRect(_Area);
		Rect rect = GetLocalRect(m_Margin);
		
		if (m_Marker != null)
			m_Marker.anchoredPosition = new Vector2(area.center.x - m_Position, 0);
		
		if (rect.Overlaps(area))
			return;
		
		m_Processed = true;
		m_Hold      = false;
		
		m_Indicator.Success(Progress, Length);
	}

	public override void TouchUp(int _ID, Rect _Area)
	{
		if (m_Processed)
			return;
		
		m_Processed = true;
		m_Hold      = false;
		
		m_Indicator.Success(Progress, Length);
	}
}