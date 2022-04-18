using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public static class QueryExtension
{
	public static async Task<DataSnapshot> GetValueAsync(this Query _Query, int _Timeout, int _Attempts = 1)
	{
		if (_Query == null)
			return null;
		
		int attempts = Mathf.Max(1, _Attempts);
		
		for (int i = 0; i < attempts; i++)
		{
			Task<DataSnapshot> fetch = _Query.GetValueAsync();
			
			await Task.WhenAny(
				fetch,
				Task.Delay(_Timeout)
			);
			
			if (!fetch.IsCanceled && !fetch.IsFaulted && fetch.IsCompleted)
				return fetch.Result;
			
			Debug.LogWarningFormat("[Query] Get value timeout ({0}ms). Attempt: {1}.", _Timeout, i + 1);
			
			await Task.Delay(250);
		}
		
		Debug.LogError("[Query] Get value failed.");
		
		return null;
	}
}