using System;
using UnityEngine;

public class UIHoldHandle : UIHandle
{
	public event Action OnStartHold;
	public event Action OnStopHold;

	[SerializeField] RectTransform    m_Marker;
	[SerializeField] UISplineProgress m_ProgressBar;
	[SerializeField] RectOffset       m_Margin;

	bool    m_Interactable;
	bool    m_Processed;
	bool    m_Hold;
	float   m_Progress;
	Vector2 m_Position;

	public override void StartReceiveInput()
	{
		if (m_Interactable)
			return;
		
		m_Interactable = true;
		m_Processed    = false;
		m_Hold         = false;
		m_Progress     = 0;
		
		if (m_ProgressBar != null)
			m_ProgressBar.gameObject.SetActive(false);
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
		
		if (m_ProgressBar != null)
			m_ProgressBar.gameObject.SetActive(false);
	}

	public void Progress(float _Progress)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		if (m_Hold)
		{
			m_ProgressBar.Max = _Progress;
			m_ProgressBar.gameObject.SetActive(true);
		}
		else
		{
			m_ProgressBar.Min = _Progress;
			m_ProgressBar.Max = _Progress;
			return;
		}
		
		m_Progress = _Progress;
		
		if (Mathf.Approximately(m_Progress, 1))
		{
			m_Processed = true;
			InvokeSuccess();
		}
	}

	public override void TouchDown(int _ID, Vector2 _Position)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		m_Hold     = true;
		m_Position = GetLocalPoint(_Position);
		
		InvokeStartHold();
	}

	public override void TouchUp(int _ID, Vector2 _Position)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		if (m_Marker != null)
			m_Marker.anchoredPosition = Vector2.zero;
		
		m_Hold      = false;
		m_Processed = true;
		
		InvokeStopHold();
		
		if (Mathf.Approximately(m_Progress, 1))
			InvokeSuccess();
		else
			InvokeFail();
	}

	public override void TouchMove(int _ID, Vector2 _Position)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		Rect    rect     = GetLocalRect(m_Margin);
		Vector2 position = GetLocalPoint(_Position) - m_Position;
		
		if (m_Marker != null)
			m_Marker.anchoredPosition = new Vector2(position.x, 0);
		
		if (position.x >= rect.xMin && position.x <= rect.xMax)
			return;
		
		m_Hold      = false;
		m_Processed = true;
		
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