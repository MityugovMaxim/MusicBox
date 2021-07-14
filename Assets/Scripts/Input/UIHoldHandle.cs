using System;
using UnityEngine;

public class UIHoldHandle : UIHandle
{
	public event Action OnStartHold;
	public event Action OnStopHold;

	[SerializeField] RectTransform m_Marker;
	[SerializeField] RectOffset    m_Margin;

	bool    m_Interactable;
	bool    m_Processed;
	bool    m_Hold;
	float   m_Progress;
	Vector2 m_Position;

	public void Progress(float _Progress)
	{
		if (!m_Interactable || !m_Hold || m_Processed)
			return;
		
		m_Progress = _Progress;
		
		if (Mathf.Approximately(m_Progress, 1))
		{
			m_Processed = true;
			InvokeSuccess();
		}
	}

	public override void StartReceiveInput()
	{
		if (m_Interactable)
			return;
		
		m_Interactable = true;
		m_Processed    = false;
		m_Hold         = false;
		m_Progress     = 0;
	}

	public override void StopReceiveInput()
	{
		if (!m_Interactable)
			return;
		
		if (m_Marker != null)
			m_Marker.anchoredPosition = Vector2.zero;
		
		if (!m_Processed)
		{
			if (Mathf.Approximately(m_Progress, 1))
				InvokeSuccess();
			else
				InvokeFail();
		}
		
		m_Interactable = false;
		m_Processed    = false;
		m_Hold         = false;
		m_Progress     = 0;
	}

	public override void TouchDown(Vector2 _Position)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		m_Hold     = true;
		m_Position = RectTransform.InverseTransformPoint(_Position);
		
		InvokeStartHold();
	}

	public override void TouchUp(Vector2 _Position)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		if (m_Marker != null)
			m_Marker.anchoredPosition = Vector2.zero;
		
		m_Processed = true;
		m_Hold      = false;
		
		InvokeStopHold();
		
		if (Mathf.Approximately(m_Progress, 1))
			InvokeSuccess();
		else
			InvokeFail();
	}

	public override void TouchMove(Vector2 _Position)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		Rect    rect     = GetLocalRect(m_Margin);
		Vector2 position = GetLocalPoint(_Position) - m_Position;
		
		if (m_Marker != null)
			m_Marker.anchoredPosition = new Vector2(position.x, 0);
		
		if (rect.Contains(position))
			return;
		
		m_Processed = true;
		m_Hold      = false;
		
		InvokeStopHold();
		
		if (Mathf.Approximately(m_Progress, 1))
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