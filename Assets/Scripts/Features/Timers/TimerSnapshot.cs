using System.Collections.Generic;
using AudioBox.Compression;
using Firebase.Database;
using JetBrains.Annotations;

public class TimerSnapshot
{
	public long StartTimestamp { [UsedImplicitly] get; }
	public long EndTimestamp   { [UsedImplicitly] get; }

	readonly Dictionary<string, object> m_Payload;

	public TimerSnapshot(DataSnapshot _Data)
	{
		StartTimestamp = _Data.GetLong("start_time");
		EndTimestamp   = _Data.GetLong("end_time");
		m_Payload      = _Data.Child("payload").GetValue(true) as Dictionary<string, object>;
	}

	public string GetString(string _Key, string _Default = null) => m_Payload.GetString(_Key, _Default);

	public int GetInteger(string _Key, int _Default = 0) => m_Payload.GetInt(_Key, _Default);

	public float GetFloat(string _Key, float _Default = 0) => m_Payload.GetFloat(_Key, _Default);

	public double GetDouble(string _Key, double _Default = 0) => m_Payload.GetDouble(_Key, _Default);

	public long GetLong(string _Key, long _Default = 0) => m_Payload.GetLong(_Key, _Default);
}