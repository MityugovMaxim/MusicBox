using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Compression;
using Firebase.Database;
using JetBrains.Annotations;
using Zenject;

public class TimerSnapshot : Snapshot
{
	public long StartTimestamp { [UsedImplicitly] get; }
	public long EndTimestamp   { [UsedImplicitly] get; }

	readonly Dictionary<string, object> m_Payload;

	public TimerSnapshot(string _ID, int _Order) : base(_ID, _Order)
	{
		StartTimestamp = 0;
		EndTimestamp   = 0;
		m_Payload      = new Dictionary<string, object>();
	}

	public TimerSnapshot(DataSnapshot _Data) : base(_Data)
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

public class TimersDataUpdateSignal { }

public class TimerStartSignal
{
	public string TimerID { get; }

	public TimerStartSignal(string _TimerID)
	{
		TimerID = _TimerID;
	}
}

public class TimerEndSignal
{
	public string TimerID { get; }

	public TimerEndSignal(string _TimerID)
	{
		TimerID = _TimerID;
	}
}

public class TimersProcessor : DataProcessor<TimerSnapshot, TimersDataUpdateSignal>
{
	protected override string Path => $"profiles/{m_SocialProcessor.UserID}/timers";

	protected override bool SupportsDevelopment => false;

	[Inject] SocialProcessor m_SocialProcessor;

	CancellationTokenSource m_TokenSource;

	protected override Task OnFetch()
	{
		ProcessTimers();
		
		return base.OnFetch();
	}

	protected override Task OnUpdate()
	{
		ProcessTimers();
		
		return base.OnUpdate();
	}

	void ProcessTimers()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		long timestamp = TimeUtility.GetTimestamp();
		
		foreach (TimerSnapshot snapshot in Snapshots)
		{
			if (snapshot == null || snapshot.StartTimestamp > timestamp || snapshot.EndTimestamp < timestamp)
				continue;
			
			int startDelay = (int)(timestamp - snapshot.StartTimestamp);
			int endDelay   = (int)(snapshot.EndTimestamp - timestamp);
			
			string timerID = snapshot.ID;
			
			if (startDelay > 0)
			{
				Task.Delay(startDelay, m_TokenSource.Token).Dispatch(
					_Task =>
					{
						if (_Task.IsCompletedSuccessfully)
							SignalBus.Fire(new TimerStartSignal(timerID));
					}
				);
			}
			
			if (endDelay > 0)
			{
				Task.Delay(endDelay, m_TokenSource.Token).Dispatch(
					_Task =>
					{
						if (_Task.IsCompletedSuccessfully)
							SignalBus.Fire(new TimerEndSignal(timerID));
					}
				);
			}
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
