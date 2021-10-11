using System;
using System.Threading.Tasks;
using Firebase.Functions;

public class TimeProcessor
{
	public async Task<long> GetServerTimestamp()
	{
		try
		{
			HttpsCallableReference getServerTime = FirebaseFunctions.DefaultInstance.GetHttpsCallable("getServerTime");
			
			HttpsCallableResult result = await getServerTime.CallAsync();
			
			return (long)result.Data;
		}
		catch (Exception)
		{
			return long.MaxValue;
		}
	}

	public async Task<DateTime> GetServerDateTime()
	{
		long timestamp = await GetServerTimestamp();
		
		DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
		
		return offset.DateTime;
	}

	public async Task<DateTime> GetLocalDateTime()
	{
		long timestamp = await GetServerTimestamp();
		
		DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
		
		return offset.LocalDateTime;
	}
}