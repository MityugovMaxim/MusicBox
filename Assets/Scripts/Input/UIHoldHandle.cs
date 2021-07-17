using System;
using UnityEngine;

public class UIHoldHandle : UIHandle
{
	public override float Progress => MaxProgress - MinProgress;

	public event Action OnStartHold;
	public event Action OnStopHold;

	public float MinProgress => m_MinProgress;
	public float MaxProgress => m_MaxProgress;

	[SerializeField] RectTransform m_Marker;
	[SerializeField] RectOffset    m_Margin;

	bool  m_Interactable;
	bool  m_Processed;
	bool  m_Hold;
	float m_MinProgress;
	float m_MaxProgress;
	Rect  m_Area;

	public override void StartReceiveInput()
	{
		if (m_Interactable)
			return;
		
		m_Interactable = true;
		m_Processed    = false;
		m_Hold         = false;
		m_MinProgress  = 0;
		m_MaxProgress  = 0;
	}

	public override void StopReceiveInput()
	{
		if (!m_Interactable)
			return;
		
		if (m_Marker != null)
			m_Marker.anchoredPosition = Vector2.zero;
		
		if (!m_Processed)
		{
			if (m_MaxProgress >= 1 || Mathf.Approximately(m_MaxProgress, 1))
				InvokeSuccess();
			else
				InvokeFail();
		}
		
		m_Interactable = false;
		m_Processed    = false;
		m_Hold         = false;
		m_MinProgress  = 0;
		m_MaxProgress  = 0;
	}

	public void Process(float _Phase)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		if (m_Hold)
		{
			m_MaxProgress = _Phase;
			
			if (m_MaxProgress >= 1 || Mathf.Approximately(m_MaxProgress, 1))
			{
				m_Hold      = false;
				m_Processed = true;
				InvokeSuccess();
			}
		}
		else
		{
			m_MinProgress = _Phase;
			m_MaxProgress = _Phase;
		}
	}

	public override void TouchDown(int _ID, Rect _Area)
	{
		if (!m_Interactable || m_Processed || m_Hold)
			return;
		
		m_Hold = true;
		m_Area = GetLocalRect(_Area);
		
		InvokeStartHold();
	}

	public override void TouchUp(int _ID, Rect _Area)
	{
		if (!m_Interactable || m_Processed || !m_Hold)
			return;
		
		if (m_Marker != null)
			m_Marker.anchoredPosition = Vector2.zero;
		
		m_Hold      = false;
		m_Processed = true;
		
		InvokeStopHold();
		
		if (m_MaxProgress >= 1 || Mathf.Approximately(m_MaxProgress, 1))
			InvokeSuccess();
		else
			InvokeFail();
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
		
		InvokeStopHold();
		
		if (m_MaxProgress >= 1 || Mathf.Approximately(m_MaxProgress, 1))
			InvokeSuccess();
		else
			InvokeFail();
	}

	void InvokeStartHold()
	{
		OnStartHold?.Invoke();
	}

	void InvokeStopHold()
	{
		OnStopHold?.Invoke();
	}
}