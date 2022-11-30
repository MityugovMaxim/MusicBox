using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[ExecuteAlways]
public class UIAnalogTimer : UIEntity
{
	public long Timestamp
	{
		get => m_Timestamp;
		set
		{
			if (m_Timestamp == value)
				return;
			
			m_Timestamp = value;
			
			ProcessTimer(true);
		}
	}

	[SerializeField] RectTransform m_Content;
	[SerializeField] UIAnalogDigit m_Hour1;
	[SerializeField] UIAnalogDigit m_Hour2;
	[SerializeField] UIAnalogDigit m_Minute1;
	[SerializeField] UIAnalogDigit m_Minute2;
	[SerializeField] UIAnalogDigit m_Second1;
	[SerializeField] UIAnalogDigit m_Second2;

	long m_Timestamp;

	IEnumerator             m_TickRoutine;
	CancellationTokenSource m_TokenSource;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		if (!Application.isPlaying)
			return;
		
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

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		
		Vector2 size = m_Content.sizeDelta;
		
		Rect rect = GetLocalRect();
		
		float scale = Mathf.Min(
			rect.width / size.x,
			rect.height / size.y
		);
		
		m_Content.localScale = new Vector3(scale, scale, 1);
		
		m_Content.ForceUpdateRectTransforms();
	}

	public void Setup(long _Timestamp)
	{
		Timestamp = _Timestamp;
		
		if (gameObject.activeInHierarchy)
			TickTimer();
	}

	void TickTimer()
	{
		if (m_TickRoutine != null)
			StopCoroutine(m_TickRoutine);
		
		m_TickRoutine = TickRoutine();
		
		StartCoroutine(m_TickRoutine);
	}

	IEnumerator TickRoutine()
	{
		long timestamp = TimeUtility.GetTimestamp();
		
		while (timestamp <= m_Timestamp)
		{
			float delay = (1000 - timestamp % 1000) * 0.001f;
			
			yield return new WaitForSeconds(delay);
			
			ProcessTimer();
			
			timestamp = TimeUtility.GetTimestamp();
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
		
		long timer = m_Timestamp - TimeUtility.GetTimestamp();
		
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
