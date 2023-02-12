using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Functions;

public abstract class FunctionRequest<TResult>
{
	protected abstract string Command { get; }

	public async Task<TResult> SendAsync(int _Timeout = 15000)
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		Serialize(data);
		
		HttpsCallableReference function = FirebaseFunctions.DefaultInstance.GetHttpsCallable(Command);
		
		try
		{
			Task<HttpsCallableResult> task = function.CallAsync(data);
			
			await Task.WhenAny(
				task,
				Task.Delay(_Timeout)
			);
			
			if (task.IsCompletedSuccessfully)
				return Success(Deserialize(task.Result.Data));
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Request failed. Command: '{0}'", Command);
		}
		
		return Fail();
	}

	protected abstract void Serialize(IDictionary<string, object> _Data);

	object Deserialize(object _Data)
	{
		if (_Data is Dictionary<object, object> data)
		{
			Dictionary<string, object> packet = new Dictionary<string, object>();
			foreach (var entry in data)
			{
				string key   = entry.Key.ToString();
				object value = entry.Value;
				packet[key] = value;
			}
			return packet;
		}
		return _Data;
	}

	protected abstract TResult Success(object _Data);

	protected abstract TResult Fail();
}
