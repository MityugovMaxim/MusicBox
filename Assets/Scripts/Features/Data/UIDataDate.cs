using System;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

public class UIDataDate : UIDataEntity
{
	[Preserve]
	public class Pool : UIDataEntityPool<UIDataDate> { }

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

	[SerializeField] TMP_Text m_YearLabel;
	[SerializeField] TMP_Text m_MonthLabel;
	[SerializeField] TMP_Text m_DayLabel;
	[SerializeField] TMP_Text m_HourLabel;
	[SerializeField] TMP_Text m_MinuteLabel;
	[SerializeField] TMP_Text m_SecondLabel;
	[SerializeField] Button   m_IncrementYearButton;
	[SerializeField] Button   m_DecrementYearButton;
	[SerializeField] Button   m_IncrementMonthButton;
	[SerializeField] Button   m_DecrementMonthButton;
	[SerializeField] Button   m_IncrementDayButton;
	[SerializeField] Button   m_DecrementDayButton;
	[SerializeField] Button   m_IncrementHourButton;
	[SerializeField] Button   m_DecrementHourButton;
	[SerializeField] Button   m_IncrementMinuteButton;
	[SerializeField] Button   m_DecrementMinuteButton;
	[SerializeField] Button   m_IncrementSecondButton;
	[SerializeField] Button   m_DecrementSecondButton;

	int m_Year  = 1970;
	int m_Month = 1;
	int m_Day   = 1;
	int m_Hour;
	int m_Minute;
	int m_Second;

	protected override void Awake()
	{
		base.Awake();
		
		m_IncrementYearButton.Subscribe(IncrementYear);
		m_DecrementYearButton.Subscribe(DecrementYear);
		m_IncrementMonthButton.Subscribe(IncrementMonth);
		m_DecrementMonthButton.Subscribe(DecrementMonth);
		m_IncrementDayButton.Subscribe(IncrementDay);
		m_DecrementDayButton.Subscribe(DecrementDay);
		m_IncrementHourButton.Subscribe(IncrementHour);
		m_DecrementHourButton.Subscribe(DecrementHour);
		m_IncrementMinuteButton.Subscribe(IncrementMinute);
		m_DecrementMinuteButton.Subscribe(DecrementMinute);
		m_IncrementSecondButton.Subscribe(IncrementSecond);
		m_DecrementSecondButton.Subscribe(DecrementSecond);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_IncrementYearButton.Unsubscribe(IncrementYear);
		m_DecrementYearButton.Unsubscribe(DecrementYear);
		m_IncrementMonthButton.Unsubscribe(IncrementMonth);
		m_DecrementMonthButton.Unsubscribe(DecrementMonth);
		m_IncrementDayButton.Unsubscribe(IncrementDay);
		m_DecrementDayButton.Unsubscribe(DecrementDay);
		m_IncrementHourButton.Unsubscribe(IncrementHour);
		m_DecrementHourButton.Unsubscribe(DecrementHour);
		m_IncrementMinuteButton.Unsubscribe(IncrementMinute);
		m_DecrementMinuteButton.Unsubscribe(DecrementMinute);
		m_IncrementSecondButton.Unsubscribe(IncrementSecond);
		m_DecrementSecondButton.Unsubscribe(DecrementSecond);
	}

	void IncrementYear() => SetYear(m_Year + 1);

	void DecrementYear() => SetYear(m_Year - 1);

	void IncrementMonth() => SetMonth(m_Month + 1);

	void DecrementMonth() => SetMonth(m_Month - 1);

	void IncrementDay() => SetDay(m_Day + 1);

	void DecrementDay() => SetDay(m_Day - 1);

	void IncrementHour() => SetHour(m_Hour + 1);

	void DecrementHour() => SetHour(m_Hour - 1);

	void IncrementMinute() => SetMinute(m_Minute + 1);

	void DecrementMinute() => SetMinute(m_Minute - 1);

	void IncrementSecond() => SetSecond(m_Second + 1);

	void DecrementSecond() => SetSecond(m_Second - 1);

	void SetYear(int _Year)
	{
		m_Year = _Year;
		
		m_YearLabel.text = m_Year.ToString();
		
		ProcessTimestamp();
	}

	void SetMonth(int _Month)
	{
		m_Month = MathUtility.Repeat(_Month, 1, 12);
		
		m_MonthLabel.text = m_Months[m_Month - 1];
		
		ProcessTimestamp();
	}

	void SetDay(int _Day)
	{
		m_Day = MathUtility.Repeat(_Day, 1, DateTime.DaysInMonth(m_Year, m_Month));
		
		m_DayLabel.text = m_Day.ToString();
		
		ProcessTimestamp();
	}

	void SetHour(int _Hours)
	{
		m_Hour = MathUtility.Repeat(_Hours, 24);
		
		m_HourLabel.text = m_Hour.ToString("00");
		
		ProcessTimestamp();
	}

	void SetMinute(int _Minutes)
	{
		m_Minute = MathUtility.Repeat(_Minutes, 60);
		
		m_MinuteLabel.text = m_Minute.ToString("00");
		
		ProcessTimestamp();
	}

	void SetSecond(int _Second)
	{
		m_Second = MathUtility.Repeat(_Second, 60);
		
		m_SecondLabel.text = m_Second.ToString("00");
		
		ProcessTimestamp();
	}

	void ProcessTimestamp()
	{
		long timestamp = TimeUtility.GetTimestamp(m_Year, m_Month, m_Day, m_Hour, m_Minute, m_Second);
		
		SetValue(timestamp);
	}

	protected override void ProcessValue()
	{
		base.ProcessValue();
		
		long value = GetValue<long>();
		
		DateTime date = TimeUtility.GetUtcTime(value);
		
		m_Year   = date.Year;
		m_Month  = date.Month;
		m_Day    = date.Day;
		m_Hour   = date.Hour;
		m_Minute = date.Minute;
		m_Second = date.Second;
		
		SetYear(date.Year);
		SetMonth(date.Month);
		SetDay(date.Day);
		SetHour(date.Hour);
		SetMinute(date.Minute);
		SetSecond(date.Second);
	}
}
