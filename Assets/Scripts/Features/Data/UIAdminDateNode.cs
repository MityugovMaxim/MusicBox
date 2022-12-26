using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAdminDateNode : UIAdminNode
{
	public class Pool : UIAdminNodePool { }

	static readonly string[] m_Months =
	{
		"JANUARY",
		"FEBRUARY",
		"MARCH",
		"APRIL",
		"MAY",
		"JUNE",
		"JULY",
		"AUGUST",
		"SEPTEMBER",
		"OCTOBER",
		"NOVEMBER",
		"DECEMBER",
	};

	[SerializeField] UIAdminField m_YearField;
	[SerializeField] TMP_Text     m_MonthLabel;
	[SerializeField] Button       m_IncrementMonthButton;
	[SerializeField] Button       m_DecrementMonthButton;
	[SerializeField] UIAdminField m_DayField;
	[SerializeField] UIAdminField m_HourField;
	[SerializeField] UIAdminField m_MinuteField;
	[SerializeField] UIAdminField m_SecondField;
	[SerializeField] Button       m_CurrentYearButton;
	[SerializeField] Button       m_CurrentDateButton;
	[SerializeField] Button       m_CurrentTimeButton;

	AdminNumberNode m_Node;

	int m_Year;
	int m_Month;
	int m_Day;
	int m_Hour;
	int m_Minute;
	int m_Second;

	protected override void Awake()
	{
		base.Awake();
		
		m_YearField.Subscribe(ProcessYear);
		m_IncrementMonthButton.Subscribe(IncrementMonth);
		m_DecrementMonthButton.Subscribe(DecrementMonth);
		m_DayField.Subscribe(ProcessDay);
		m_HourField.Subscribe(ProcessHour);
		m_MinuteField.Subscribe(ProcessMinute);
		m_SecondField.Subscribe(ProcessSecond);
		m_CurrentYearButton.Subscribe(CurrentYear);
		m_CurrentDateButton.Subscribe(CurrentDate);
		m_CurrentTimeButton.Subscribe(CurrentTime);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_YearField.Unsubscribe(ProcessYear);
		m_IncrementMonthButton.Unsubscribe(IncrementMonth);
		m_DecrementMonthButton.Unsubscribe(DecrementMonth);
		m_DayField.Unsubscribe(ProcessDay);
		m_HourField.Unsubscribe(ProcessHour);
		m_MinuteField.Unsubscribe(ProcessMinute);
		m_SecondField.Unsubscribe(ProcessSecond);
		m_CurrentYearButton.Unsubscribe(CurrentYear);
		m_CurrentDateButton.Unsubscribe(CurrentDate);
		m_CurrentTimeButton.Unsubscribe(CurrentTime);
	}

	public override void Setup(UIAdminNode _Parent, AdminNode _Node)
	{
		base.Setup(_Parent, _Node);
		
		m_Node = Node as AdminNumberNode;
		
		ValueChanged();
	}

	void ProcessYear(string _Value)
	{
		if (!string.IsNullOrEmpty(_Value) && int.TryParse(_Value, out int year))
			m_Year = year;
		
		ProcessTimestamp();
	}

	void IncrementMonth()
	{
		m_Month = MathUtility.Repeat(m_Month + 1, 1, 12);
		
		m_MonthLabel.text = m_Months[m_Month - 1];
		
		ProcessTimestamp();
	}

	void DecrementMonth()
	{
		m_Month = MathUtility.Repeat(m_Month - 1, 1, 12);
		
		ProcessTimestamp();
	}

	void ProcessDay(string _Value)
	{
		if (!string.IsNullOrEmpty(_Value) && int.TryParse(_Value, out int day))
			m_Day = day;
		
		ProcessTimestamp();
	}

	void ProcessHour(string _Value)
	{
		if (!string.IsNullOrEmpty(_Value) && int.TryParse(_Value, out int hour))
			m_Hour = hour;
		
		ProcessTimestamp();
	} 

	void ProcessMinute(string _Value)
	{
		if (!string.IsNullOrEmpty(_Value) && int.TryParse(_Value, out int minute))
			m_Minute = minute;
		
		ProcessTimestamp();
	}

	void ProcessSecond(string _Value)
	{
		if (!string.IsNullOrEmpty(_Value) && int.TryParse(_Value, out int second))
			m_Second = second;
		
		ProcessTimestamp();
	}

	void CurrentYear()
	{
		m_Year = DateTime.UtcNow.Year;
		
		ProcessTimestamp();
	}

	void CurrentDate()
	{
		DateTime date = DateTime.UtcNow;
		
		m_Month = date.Month;
		m_Day   = date.Day;
		
		ProcessTimestamp();
	}

	void CurrentTime()
	{
		DateTime date = DateTime.UtcNow;
		
		m_Hour   = date.Hour;
		m_Minute = date.Minute;
		m_Second = date.Second;
		
		ProcessTimestamp();
	}

	void ProcessTimestamp()
	{
		long timestamp = TimeUtility.GetTimestamp(m_Year, m_Month, m_Day, m_Hour, m_Minute, m_Second);
		
		DateTime date = TimeUtility.GetUtcTime(timestamp);
		
		m_Year   = date.Year;
		m_Month  = date.Month;
		m_Day    = date.Day;
		m_Hour   = date.Hour;
		m_Minute = date.Minute;
		m_Second = date.Second;
		
		ProcessDate();
		
		m_Node.Value = timestamp;
	}

	void ProcessDate()
	{
		m_YearField.Value   = m_Year.ToString("0000");
		m_MonthLabel.text   = m_Months[m_Month - 1];
		m_DayField.Value    = m_Day.ToString("00");
		m_HourField.Value   = m_Hour.ToString("00");
		m_MinuteField.Value = m_Minute.ToString("00");
		m_SecondField.Value = m_Second.ToString("00");
	}

	protected override void ValueChanged()
	{
		long timestamp = (long)m_Node.Value;
		
		DateTime date = TimeUtility.GetUtcTime(timestamp);
		
		m_Year   = date.Year;
		m_Month  = date.Month;
		m_Day    = date.Day;
		m_Hour   = date.Hour;
		m_Minute = date.Minute;
		m_Second = date.Second;
		
		ProcessDate();
	}
}
