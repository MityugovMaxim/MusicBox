using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UITimer : UIEntity, IPointerDownHandler, ISampleReceiver
{
	public enum TimerMode
	{
		ActualTime    = 0,
		EstimatedTime = 1,
	}

	const string TIMER_MODE_KEY = "timer_mode";

	[SerializeField] TMP_Text  m_Label;
	[SerializeField] TimerMode m_Mode = TimerMode.ActualTime;

	float m_Time;
	float m_Length;

	protected override void Awake()
	{
		base.Awake();
		
		m_Time   = 0;
		m_Length = 0;
		
		LoadMode();
	}

	public void Sample(float _Time, float _Length)
	{
		m_Time   = _Time;
		m_Length = _Length;
		
		float time = Mathf.Floor(m_Time);
		
		if (m_Mode == TimerMode.EstimatedTime)
			time = Mathf.Abs(time - m_Length);
		
		int minutes = Mathf.FloorToInt(time / 60);
		int seconds = Mathf.FloorToInt(time % 60);
		
		m_Label.text =  m_Mode == TimerMode.ActualTime
			? $"{minutes}:{seconds:00}"
			: $"-{minutes}:{seconds:00}";
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData _EventData)
	{
		switch (m_Mode)
		{
			case TimerMode.ActualTime:
				m_Mode = TimerMode.EstimatedTime;
				break;
			case TimerMode.EstimatedTime:
				m_Mode = TimerMode.ActualTime;
				break;
		}
		
		Sample(m_Time, m_Length);
		
		SaveMode();
	}

	void LoadMode()
	{
		m_Mode = (TimerMode)PlayerPrefs.GetInt(TIMER_MODE_KEY, (int)TimerMode.ActualTime);
	}

	void SaveMode()
	{
		PlayerPrefs.SetInt(TIMER_MODE_KEY, (int)m_Mode);
	}
}
