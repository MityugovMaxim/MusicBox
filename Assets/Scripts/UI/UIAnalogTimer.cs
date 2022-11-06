using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UIAnalogTimer : UIEntity
{
	[SerializeField] UIAnalogDigit m_Hour1;
	[SerializeField] UIAnalogDigit m_Hour2;
	[SerializeField] UIAnalogDigit m_Minute1;
	[SerializeField] UIAnalogDigit m_Minute2;
	[SerializeField] UIAnalogDigit m_Second1;
	[SerializeField] UIAnalogDigit m_Second2;

	long m_SourceTimestamp;
	long m_TargetTimestamp;

	CancellationTokenSource m_TokenSource;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		Setup(
			TimeUtility.GetTimestamp(),
			TimeUtility.GetTimestamp() + 180000
		);
		
		ProcessTimer(true);
		
		TickTimer();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_TokenSource?.Cancel();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_TokenSource?.Cancel();
	}

	public void Setup(long _Timestamp) => Setup(TimeUtility.GetTimestamp(), _Timestamp);

	public void Setup(long _SourceTimestamp, long _TargetTimestamp)
	{
		m_SourceTimestamp = _SourceTimestamp;
		m_TargetTimestamp = _TargetTimestamp;
		
		ProcessTimer(true);
	}

	async void TickTimer()
	{
		while (true)
		{
			await Task.Delay(1000);
			
			ProcessTimer();
			
			if (TimeUtility.GetTimestamp() >= m_TargetTimestamp)
				break;
		}
		
		ProcessTimer(true);
	}

	async void ProcessTimer(bool _Instant = false)
	{
		await ProcessTimerAsync(_Instant);
	}

	async Task ProcessTimerAsync(bool _Instant = false)
	{
		m_TokenSource?.Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		long timer = m_TargetTimestamp - m_SourceTimestamp - (TimeUtility.GetTimestamp() - m_SourceTimestamp);
		
		TimeSpan time = TimeSpan.FromMilliseconds(timer);
		
		int hours   = time.Hours < 100 ? time.Hours : 99;
		int minutes = time.Minutes;
		int seconds = time.Seconds;
		
		try
		{
			await Task.WhenAll(
				m_Hour1.SetValueAsync(hours % 10, _Instant, m_TokenSource.Token),
				m_Hour2.SetValueAsync(hours / 10 % 10, _Instant, m_TokenSource.Token),
				m_Minute1.SetValueAsync(minutes % 10, _Instant, m_TokenSource.Token),
				m_Minute2.SetValueAsync(minutes / 10 % 10, _Instant, m_TokenSource.Token),
				m_Second1.SetValueAsync(seconds % 10, _Instant, m_TokenSource.Token),
				m_Second2.SetValueAsync(seconds / 10 % 10, _Instant, m_TokenSource.Token)
			);
		}
		catch (TaskCanceledException) { }
		catch (OperationCanceledException) { }
		finally
		{
			m_TokenSource?.Dispose();
			m_TokenSource = null;
		}
	}
}
