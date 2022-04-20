using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIMultiplierIndicator : UIEntity
{
	[SerializeField] UIMultiplierProgress m_MultiplierProgress;
	[SerializeField] UIMultiplierLabel    m_MultiplierLabel;

	[SerializeField, Sound] string m_MultiplierX2Sound;
	[SerializeField, Sound] string m_MultiplierX4Sound;
	[SerializeField, Sound] string m_MultiplierX6Sound;
	[SerializeField, Sound] string m_MultiplierX8Sound;

	[Inject] SignalBus      m_SignalBus;
	[Inject] SoundProcessor m_SoundProcessor;

	int   m_Multiplier;
	float m_Progress;

	CancellationTokenSource m_TokenSource;

	protected override void Awake()
	{
		base.Awake();
		
		m_Multiplier = 1;
		m_Progress   = 0;
		
		m_MultiplierLabel.Multiplier  = m_Multiplier;
		m_MultiplierProgress.Progress = m_Progress;
		
		m_SignalBus.Subscribe<ScoreSignal>(RegisterScore);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_SignalBus.Unsubscribe<ScoreSignal>(RegisterScore);
	}

	void RegisterScore(ScoreSignal _Signal)
	{
		ProcessMultiplier(_Signal.Multiplier, _Signal.Progress);
	}

	async void ProcessMultiplier(int _Multiplier, float _Progress)
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		List<Task> tasks = new List<Task>();
		if (_Multiplier > m_Multiplier)
		{
			string multiplierSound = GetMultiplierSound(_Multiplier);
			
			m_MultiplierLabel.Multiplier = _Multiplier;
			tasks.Add(m_SoundProcessor.PlayAsync(multiplierSound));
			tasks.Add(m_MultiplierProgress.PlayAsync());
			tasks.Add(m_MultiplierLabel.PlayAsync());
		}
		else if (_Multiplier < m_Multiplier)
		{
			m_MultiplierLabel.Restore();
			m_MultiplierLabel.Multiplier  = _Multiplier;
			m_MultiplierProgress.Progress = _Progress;
		}
		
		if (_Progress > m_Progress)
		{
			tasks.Add(m_MultiplierProgress.ProgressAsync(_Progress, token));
		}
		else if (_Progress < m_Progress)
		{
			m_MultiplierProgress.Progress = _Progress;
		}
		
		m_Progress   = _Progress;
		m_Multiplier = _Multiplier;
		
		try
		{
			await Task.WhenAll(tasks);
		}
		catch (TaskCanceledException) { }
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	string GetMultiplierSound(int _Multiplier)
	{
		if (_Multiplier <= 2)
			return m_MultiplierX2Sound;
		else if (_Multiplier <= 4)
			return m_MultiplierX4Sound;
		else if (_Multiplier <= 6)
			return m_MultiplierX6Sound;
		else
			return m_MultiplierX8Sound;
	}
}