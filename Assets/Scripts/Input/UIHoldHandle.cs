using System;
using UnityEngine;
using UnityEngine.Events;

public class UIHoldHandle : UIHandle
{
	[Serializable]
	public class HoldEvent : UnityEvent<bool> { }

	public float MinProgress { get; private set; }

	public float MaxProgress { get; private set; }

	protected override bool Processed => m_Processed;

	[SerializeField] RectTransform m_Marker;
	[SerializeField] RectOffset    m_Margin;

	UIHoldIndicator m_Indicator;
	bool            m_Interactable;
	bool            m_Processed;
	bool            m_Hold;
	bool            m_Miss;
	Rect            m_Area;

	public void Setup(UIHoldIndicator _Indicator)
	{
		m_Indicator = _Indicator;
		
		MinProgress = 0;
		MaxProgress = 0;
	}

	public override void StartReceiveInput()
	{
		if (m_Interactable)
			return;
		
		m_Interactable = true;
		m_Processed    = false;
		m_Hold         = false;
		m_Miss         = false;
		MinProgress    = 0;
		MaxProgress    = 0;
	}

	public override void StopReceiveInput()
	{
		if (!m_Interactable)
			return;
		
		if (m_Marker != null)
			m_Marker.anchoredPosition = Vector2.zero;
		
		if (!m_Processed)
			ProcessFail();
		
		m_Interactable = false;
		m_Processed    = false;
		m_Hold         = false;
		m_Miss         = false;
	}

	public void Process(float _Phase)
	{
		if (!m_Interactable || m_Processed)
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
			
			Vector2 position = m_Indicator.GetMinPosition();
			Rect    rect     = GetWorldRect();
			
			if (!m_Miss && !rect.Contains(position))
			{
				m_Miss = true;
				
				ProcessMiss();
			}
		}
	}

	public override void TouchDown(int _ID, Rect _Area)
	{
		if (!m_Interactable || m_Processed || m_Hold)
			return;
		
		m_Hold = true;
		m_Area = GetLocalRect(_Area);
		
		ProcessHit();
	}

	public override void TouchUp(int _ID, Rect _Area)
	{
		if (!m_Interactable || m_Processed || !m_Hold)
			return;
		
		if (m_Marker != null)
			m_Marker.anchoredPosition = Vector2.zero;
		
		m_Hold      = false;
		m_Processed = true;
		
		Vector2 position = m_Indicator.GetMaxPosition();
		Rect    rect     = GetWorldRect();
		
		if (rect.Contains(position))
			ProcessSuccess();
		else
			ProcessFail();
	}

	public override void TouchMove(int _ID, Rect _Area)
	{
		if (!m_Interactable || m_Processed || !m_Hold)
			return;
		
		Rect rect = GetLocalRect(m_Margin);
		Rect area = GetLocalRect(_Area);
		
		if (m_Marker != null)
			m_Marker.anchoredPosition = new Vector2(area.center.x - m_Area.center.x, 0);
		
		if (rect.Overlaps(area))
			return;
		
		m_Hold      = false;
		m_Processed = true;
		
		if (MaxProgress >= 1 || Mathf.Approximately(MaxProgress, 1))
			ProcessSuccess();
		else
			ProcessFail();
	}

	void ProcessHit()
	{
		if (m_Indicator != null)
			m_Indicator.Hit(MinProgress, MaxProgress);
	}

	void ProcessMiss()
	{
		if (m_Indicator != null)
			m_Indicator.Miss(MinProgress, MaxProgress);
	}

	void ProcessSuccess()
	{
		if (m_Indicator != null)
			m_Indicator.Success(MinProgress, MaxProgress);
	}

	void ProcessFail()
	{
		if (m_Indicator != null)
			m_Indicator.Fail(MinProgress, MaxProgress);
	}
}