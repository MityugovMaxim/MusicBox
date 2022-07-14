using System;

public static class TimeUtility
{
	public static long GetTimestamp()
	{
		TimeSpan time = DateTime.UtcNow - DateTime.UnixEpoch;
		
		return (long)time.TotalMilliseconds;
	}

	public static DateTime GetUtcTime(long _Timestamp)
	{
		return DateTimeOffset.FromUnixTimeMilliseconds(_Timestamp).UtcDateTime;
	}

	public static DateTime GetLocalTime(long _Timestamp)
	{
		return DateTimeOffset.FromUnixTimeMilliseconds(_Timestamp).LocalDateTime;
	}
}