using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ScheduleProcessor
{
	readonly Dictionary<string, CancellationTokenSource> m_Tokens = new Dictionary<string, CancellationTokenSource>();

	public void CancelStart(string _ID)
	{
		_ID ??= string.Empty;
		
		Cancel($"{_ID}_start");
	}

	public void CancelEnd(string _ID)
	{
		_ID ??= string.Empty;
		
		Cancel($"{_ID}_start");
	}

	public void Cancel(string _TimerID)
	{
		CancelTimer(_TimerID);
	}

	public void ScheduleStart(string _ID, long _Timestamp, DataEventHandler _Complete, DataEventHandler _Cancel)
	{
		_ID ??= string.Empty;
		
		string timerID = $"{_ID}_start";
		
		Schedule(timerID, _ID, _Timestamp, _Complete, _Cancel);
	}

	public void ScheduleEnd(string _ID, long _Timestamp, DataEventHandler _Complete, DataEventHandler _Cancel)
	{
		_ID ??= string.Empty;
		
		string timerID = $"{_ID}_end";
		
		Schedule(timerID, _ID, _Timestamp, _Complete, _Cancel);
	}

	public void Schedule(string _ID, long _Timestamp, DataEventHandler _Complete, DataEventHandler _Cancel) => Schedule(_ID, _ID, _Timestamp, _Complete, _Cancel);

	public void Schedule(string _TimerID, string _ID, long _Timestamp, DataEventHandler _Complete, DataEventHandler _Cancel)
	{
		Schedule(
			_TimerID,
			_ID,
			_Timestamp,
			_Complete != null ? _Complete.Invoke : null,
			_Cancel != null ? _Cancel.Invoke : null
		);
	}

	public void Schedule(string _TimerID, string _ID, long _Timestamp, Action<string> _Complete, Action<string> _Cancel)
	{
		if (string.IsNullOrEmpty(_TimerID))
			return;
		
		CancelTimer(_TimerID);
		
		long timestamp = TimeUtility.GetTimestamp();
		
		int delay = (int)(_Timestamp - timestamp);
		
		if (delay <= 0)
		{
			_Complete?.Invoke(_ID);
			return;
		}
		
		CancellationTokenSource source = new CancellationTokenSource();
		
		CancellationToken token = source.Token;
		
		m_Tokens[_TimerID] = source;
		
		Task.Delay(delay, token).Dispatch(
			_Task =>
			{
				if (_Task.IsCompletedSuccessfully)
				{
					CompleteTimer(_TimerID);
					_Complete?.Invoke(_ID);
				}
				else
				{
					CancelTimer(_TimerID);
					_Cancel?.Invoke(_ID);
				}
			}
		);
	}

	void CompleteTimer(string _TimerID)
	{
		if (string.IsNullOrEmpty(_TimerID) || !m_Tokens.TryGetValue(_TimerID, out CancellationTokenSource source))
			return;
		
		source?.Dispose();
		
		m_Tokens.Remove(_TimerID);
	}

	void CancelTimer(string _TimerID)
	{
		if (string.IsNullOrEmpty(_TimerID) || !m_Tokens.TryGetValue(_TimerID, out CancellationTokenSource source))
			return;
		
		if (source != null)
		{
			source.Cancel();
			source.Dispose();
		}
		
		m_Tokens.Remove(_TimerID);
	}
}