using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIReviveTimer : UIEntity
{
	[SerializeField] UISplineProgress m_Progress;
	[SerializeField] UIUnitLabel      m_Timer;
	[SerializeField] UIUnitLabel      m_Coins;
	[SerializeField] UIButton         m_SkipButton;

	[Inject] RevivesManager m_RevivesManager;

	int                        m_Count;
	bool                       m_Paused;
	float                      m_Time;
	IEnumerator                m_TimerRoutine;
	TaskCompletionSource<bool> m_Task;

	protected override void Awake()
	{
		base.Awake();
		
		m_SkipButton.Subscribe(Skip);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_SkipButton.Unsubscribe(Skip);
	}

	public Task<bool> ProcessAsync()
	{
		Cancel();
		
		m_Task = new TaskCompletionSource<bool>();
		
		m_Coins.Value = m_RevivesManager.GetCoins(m_Count);
		
		StartTimer();
		
		return m_Task.Task;
	}

	public void Complete()
	{
		m_Task.TrySetResult(true);
		m_Task = null;
	}

	public void Pause()
	{
		m_Paused = true;
	}

	public void Resume()
	{
		m_Paused = false;
	}

	void Skip()
	{
		m_Time += 1;
	}

	void Cancel()
	{
		m_Paused = false;
	}

	void StartTimer()
	{
		const float duration = 5;
		
		if (m_TimerRoutine != null)
			StopCoroutine(m_TimerRoutine);
		
		m_TimerRoutine = TimerRoutine(duration);
		
		StartCoroutine(m_TimerRoutine);
	}

	IEnumerator TimerRoutine(float _Duration)
	{
		m_Time = 0;
		
		while (m_Time < _Duration)
		{
			yield return null;
			
			if (m_Paused)
				continue;
			
			m_Time += Time.deltaTime;
			
			float phase = m_Time / _Duration;
			
			m_Progress.Max = Mathf.Lerp(1, 0, phase);
			m_Timer.Value  = Mathf.CeilToInt(Mathf.Lerp(_Duration, 0, phase));
		}
		
		m_Progress.Max = 0;
		m_Timer.Value  = 0;
		
		m_Task.TrySetResult(false);
		m_Task = null;
	}
}
