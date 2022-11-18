using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class TimersManager : ProfileCollection<TimerSnapshot>
{
	protected override string Name => "timers";

	readonly DataEventHandler m_StartHandler  = new DataEventHandler(DataEventType.None);
	readonly DataEventHandler m_EndHandler    = new DataEventHandler(DataEventType.None);
	readonly DataEventHandler m_CancelHandler = new DataEventHandler(DataEventType.None);

	readonly Dictionary<string, CancellationTokenSource> m_TimersStart = new Dictionary<string, CancellationTokenSource>();
	readonly Dictionary<string, CancellationTokenSource> m_TimersEnd   = new Dictionary<string, CancellationTokenSource>();

	public void SubscribeStart(string _TimerID, Action _Action) => m_StartHandler.AddListener(_TimerID, _Action);

	public void SubscribeEnd(string _TimerID, Action _Action) => m_EndHandler.AddListener(_TimerID, _Action);

	public void SubscribeCancel(string _TimerID, Action _Action) => m_EndHandler.AddListener(_TimerID, _Action);

	public void UnsubscribeStart(string _TimerID, Action _Action) => m_StartHandler.RemoveListener(_TimerID, _Action);

	public void UnsubscribeEnd(string _TimerID, Action _Action) => m_EndHandler.RemoveListener(_TimerID, _Action);

	public void UnsubscribeCancel(string _TimerID, Action _Action) => m_EndHandler.RemoveListener(_TimerID, _Action);

	protected override Task OnFetch()
	{
		foreach (string timerID in GetIDs())
			ProcessTimer(timerID);
		
		return base.OnFetch();
	}

	protected override void OnSnapshotAdd(string _TimerID)
	{
		ProcessTimer(_TimerID);
	}

	protected override void OnSnapshotRemove(string _TimerID)
	{
		CancelTimer(_TimerID);
	}

	protected override void OnSnapshotChange(string _TimerID)
	{
		ProcessTimer(_TimerID);
	}

	void CompleteTimerStart(string _TimerID)
	{
		if (!m_TimersStart.TryGetValue(_TimerID, out CancellationTokenSource tokenSource) || tokenSource == null)
			return;
		
		tokenSource.Dispose();
		
		m_TimersStart.Remove(_TimerID);
	}

	void CompleteTimerEnd(string _TimerID)
	{
		if (!m_TimersEnd.TryGetValue(_TimerID, out CancellationTokenSource tokenSource) || tokenSource == null)
			return;
		
		tokenSource.Dispose();
		
		m_TimersStart.Remove(_TimerID);
	}

	void CancelTimer(string _TimerID)
	{
		if (CancelTimerStart(_TimerID) | CancelTimerEnd(_TimerID))
			m_CancelHandler.Invoke(_TimerID);
	}

	bool CancelTimerStart(string _TimerID)
	{
		if (!m_TimersStart.TryGetValue(_TimerID, out CancellationTokenSource tokenSource) || tokenSource == null)
			return false;
		
		tokenSource.Cancel();
		tokenSource.Dispose();
		
		m_TimersStart.Remove(_TimerID);
		
		return true;
	}

	bool CancelTimerEnd(string _TimerID)
	{
		if (!m_TimersEnd.TryGetValue(_TimerID, out CancellationTokenSource tokenSource) || tokenSource == null)
			return false;
		
		tokenSource.Cancel();
		tokenSource.Dispose();
		
		m_TimersEnd.Remove(_TimerID);
		
		return true;
	}

	void ProcessTimer(string _TimerID)
	{
		CancelTimer(_TimerID);
		
		long timestamp = TimeUtility.GetTimestamp();
		
		TimerSnapshot snapshot = GetSnapshot(_TimerID);
		
		if (snapshot == null || snapshot.StartTimestamp < timestamp && snapshot.EndTimestamp < timestamp)
			return;
		
		int startDelay = (int)(timestamp - snapshot.StartTimestamp);
		int endDelay   = (int)(snapshot.EndTimestamp - timestamp);
		
		if (startDelay > 0)
		{
			m_TimersStart[_TimerID] = new CancellationTokenSource();
			
			Task.Delay(startDelay, m_TimersStart[_TimerID].Token).Dispatch(
				_Task =>
				{
					CompleteTimerStart(_TimerID);
					if (_Task.IsCompletedSuccessfully)
						m_StartHandler.Invoke(_TimerID);
				}
			);
		}
		
		if (endDelay > 0)
		{
			m_TimersEnd[_TimerID] = new CancellationTokenSource();
			
			Task.Delay(endDelay, m_TimersEnd[_TimerID].Token).Dispatch(
				_Task =>
				{
					CompleteTimerEnd(_TimerID);
					if (_Task.IsCompletedSuccessfully)
						m_EndHandler.Invoke(_TimerID);
				}
			);
		}
	}

	public long GetStartTimestamp(string _TimerID)
	{
		TimerSnapshot snapshot = GetSnapshot(_TimerID);
		
		return snapshot?.StartTimestamp ?? 0;
	}

	public long GetEndTimestamp(string _TimerID)
	{
		TimerSnapshot snapshot = GetSnapshot(_TimerID);
		
		return snapshot?.EndTimestamp ?? 0;
	}

	public int GetInteger(string _TimerID, string _Key, int _Default = 0)
	{
		TimerSnapshot snapshot = GetSnapshot(_TimerID);
		
		return snapshot != null ? snapshot.GetInteger(_Key, _Default) : _Default;
	}

	public float GetFloat(string _TimerID, string _Key, float _Default = 0)
	{
		TimerSnapshot snapshot = GetSnapshot(_TimerID);
		
		return snapshot != null ? snapshot.GetFloat(_Key, _Default) : _Default;
	}

	public long GetLong(string _TimerID, string _Key, long _Default = 0)
	{
		TimerSnapshot snapshot = GetSnapshot(_TimerID);
		
		return snapshot != null ? snapshot.GetLong(_Key, _Default) : _Default;
	}

	public double GetDouble(string _TimerID, string _Key, double _Default = 0)
	{
		TimerSnapshot snapshot = GetSnapshot(_TimerID);
		
		return snapshot != null ? snapshot.GetDouble(_Key, _Default) : _Default;
	}

	public string GetString(string _TimerID, string _Key, string _Default = null)
	{
		TimerSnapshot snapshot = GetSnapshot(_TimerID);
		
		return snapshot != null ? snapshot.GetString(_Key, _Default) : _Default;
	}
}
