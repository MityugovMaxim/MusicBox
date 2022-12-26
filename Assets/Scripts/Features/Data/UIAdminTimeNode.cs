using System;
using System.Globalization;
using UnityEngine;

public class UIAdminTimeNode : UIAdminNode
{
	public class Pool : UIAdminNodePool { }

	[SerializeField] UIAdminField m_DaysField;
	[SerializeField] UIAdminField m_HoursField;
	[SerializeField] UIAdminField m_MinutesField;
	[SerializeField] UIAdminField m_SecondsField;

	AdminNumberNode m_Node;

	int m_Days;
	int m_Hours;
	int m_Minutes;
	int m_Seconds;

	protected override void Awake()
	{
		base.Awake();
		
		m_DaysField.Subscribe(ProcessDays);
		m_HoursField.Subscribe(ProcessHours);
		m_MinutesField.Subscribe(ProcessMinutes);
		m_SecondsField.Subscribe(ProcessSeconds);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_DaysField.Unsubscribe(ProcessDays);
		m_HoursField.Unsubscribe(ProcessHours);
		m_MinutesField.Unsubscribe(ProcessMinutes);
		m_SecondsField.Unsubscribe(ProcessSeconds);
	}

	public override void Setup(UIAdminNode _Parent, AdminNode _Node)
	{
		base.Setup(_Parent, _Node);
		
		m_Node = Node as AdminNumberNode;
		
		ValueChanged();
	}

	void ProcessDays(string _Value)
	{
		if (!string.IsNullOrEmpty(_Value) && int.TryParse(_Value, out int days))
			m_Days = days;
		
		ProcessTimestamp();
	}

	void ProcessHours(string _Value)
	{
		if (!string.IsNullOrEmpty(_Value) && int.TryParse(_Value, out int hours))
			m_Hours = hours;
		
		ProcessTimestamp();
	}

	void ProcessMinutes(string _Value)
	{
		if (!string.IsNullOrEmpty(_Value) && int.TryParse(_Value, out int minutes))
			m_Minutes = minutes;
		
		ProcessTimestamp();
	}

	void ProcessSeconds(string _Value)
	{
		if (!string.IsNullOrEmpty(_Value) && int.TryParse(_Value, out int seconds))
			m_Seconds = seconds;
		
		ProcessTimestamp();
	}

	void ProcessTimestamp()
	{
		if (m_Node == null)
			return;
		
		TimeSpan time = new TimeSpan();
		time += TimeSpan.FromDays(m_Days);
		time += TimeSpan.FromHours(m_Hours);
		time += TimeSpan.FromMinutes(m_Minutes);
		time += TimeSpan.FromSeconds(m_Seconds);
		
		m_Days    = (int)time.TotalDays;
		m_Hours   = time.Hours;
		m_Minutes = time.Minutes;
		m_Seconds = time.Seconds;
		
		ProcessTime();
		
		m_Node.Value = (decimal)time.TotalMilliseconds;
	}

	void ProcessTime()
	{
		if (m_Node == null)
			return;
		
		TimeSpan time = new TimeSpan();
		time += TimeSpan.FromDays(m_Days);
		time += TimeSpan.FromHours(m_Hours);
		time += TimeSpan.FromMinutes(m_Minutes);
		time += TimeSpan.FromSeconds(m_Seconds);
		
		m_DaysField.Value    = time.TotalDays.ToString("00", CultureInfo.InvariantCulture);
		m_HoursField.Value   = time.Hours.ToString("00");
		m_MinutesField.Value = time.Minutes.ToString("00");
		m_SecondsField.Value = time.Seconds.ToString("00");
	}

	protected override void ValueChanged()
	{
		long timestamp = (long)m_Node.Value;
		
		TimeSpan time = TimeSpan.FromMilliseconds(timestamp);
		
		m_Days    = (int)time.TotalDays;
		m_Hours   = time.Hours;
		m_Minutes = time.Minutes;
		m_Seconds = time.Seconds;
		
		ProcessTime();
	}
}
