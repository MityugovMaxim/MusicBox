using System;
using UnityEngine;

public static class TimeUtility
{
	public static long GetTimestamp()
	{
		TimeSpan time = DateTime.UtcNow - DateTime.UnixEpoch;
		
		return (long)time.TotalMilliseconds;
	}

	public static long GetTimestamp(int _Year, int _Month, int _Day, int _Hour, int _Minute, int _Second)
	{
		DateTime unix = DateTime.UnixEpoch;
		
		int year   = Mathf.Max(_Year, unix.Year);
		int month  = Mathf.Clamp(_Month, 1, 12);
		int day    = Mathf.Clamp(_Day, 1, DateTime.DaysInMonth(year, month));
		int hour   = Mathf.Clamp(_Hour, 0, 23);
		int minute = Mathf.Clamp(_Minute, 0, 59);
		int second = Mathf.Clamp(_Second, 0, 59);
		
		DateTime date = new DateTime(year, _Month, day, hour, minute, second, DateTimeKind.Utc);
		
		TimeSpan time = date - DateTime.UnixEpoch;
		
		return (long)time.TotalMilliseconds;
	}

	public static long GetTimestamp(int _Days, int _Hours, int _Minutes)
	{
		TimeSpan time = DateTime.UtcNow - DateTime.UnixEpoch;
		
		time += new TimeSpan(_Days, _Hours, _Minutes, 0);
		
		return (long)time.TotalMilliseconds;
	}

	public static DateTime GetUtcTime(long _Timestamp)
	{
		return DateTimeOffset.FromUnixTimeMilliseconds(_Timestamp).UtcDateTime;
	}

	public static DateTime GetLocalTime(double _Timestamp) => GetLocalTime((long)_Timestamp);

	public static DateTime GetLocalTime(long _Timestamp)
	{
		return DateTimeOffset.FromUnixTimeMilliseconds(_Timestamp).LocalDateTime;
	}
}
