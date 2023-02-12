using System;
using System.Threading;
using System.Threading.Tasks;

public class ScheduleProcessor
{
	const string TAG = nameof(ScheduleProcessor);

	public void Schedule<T>(object _Context, T _Value, long _StartTimestamp, long _EndTimestamp, DataEventHandler<T> _Start, DataEventHandler<T> _End, DataEventHandler<T> _Cancel)
	{
		Schedule(
			_Context,
			$"{_Value}_id",
			_Value,
			_StartTimestamp,
			_EndTimestamp,
			_Start,
			_End,
			_Cancel
		);
	}

	public void Schedule<T>(object _Context, string _ID, T _Value, long _StartTimestamp, long _EndTimestamp, DataEventHandler<T> _Start, DataEventHandler<T> _End, DataEventHandler<T> _Cancel)
	{
		Schedule(_Context, $"{_ID}_start", _Value, _StartTimestamp, _Start, _Cancel);
		Schedule(_Context, $"{_ID}_end", _Value, _EndTimestamp, _End, _Cancel);
	}

	public void Schedule<T>(object _Context, T _Value, long _Timestamp, DataEventHandler<T> _Complete, DataEventHandler<T> _Cancel)
	{
		Schedule(
			_Context,
			$"{_Value}_id",
			_Value,
			_Timestamp,
			_Complete,
			_Cancel
		);
	}

	public void Schedule<T>(object _Context, string _ID, T _Value, long _Timestamp, DataEventHandler<T> _Complete, DataEventHandler<T> _Cancel)
	{
		Schedule(
			$"[{_Context.GetType().Name}] {_ID}",
			_Value,
			_Timestamp,
			_Complete != null ? _Complete.Invoke : null,
			_Cancel != null ? _Cancel.Invoke : null
		);
	}

	static void Schedule<T>(string _ID, T _Value, long _Timestamp, Action<T> _Complete, Action<T> _Cancel)
	{
		if (string.IsNullOrEmpty(_ID))
			return;
		
		TokenProvider.CancelToken(TAG, _ID);
		
		CancellationToken token = TokenProvider.CreateToken(TAG, _ID);
		
		long timestamp = TimeUtility.GetTimestamp();
		
		void Complete() => _Complete?.Invoke(_Value);
		
		void Cancel() => _Cancel?.Invoke(_Value);
		
		int delay = (int)(_Timestamp - timestamp);
		if (delay > 0)
			Delay(delay, Complete, Cancel, token);
		else
			_Complete?.Invoke(_Value);
	}

	static void Delay(int _Delay, Action _Complete, Action _Cancel, CancellationToken _Token = default)
	{
		Task.Delay(_Delay, _Token).Dispatch(
			_Task =>
			{
				if (_Task.IsCompletedSuccessfully)
					_Complete?.Invoke();
				else
					_Cancel?.Invoke();
			},
			CancellationToken.None
		);
	}
}
