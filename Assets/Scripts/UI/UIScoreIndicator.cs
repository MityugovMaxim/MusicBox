using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIScoreIndicator : UIEntity
{
	[SerializeField] UIUnitLabel m_ScoreLabel;
	[SerializeField] float       m_Duration = 0.15f;

	[Inject] SignalBus m_SignalBus;

	long m_Score;

	CancellationTokenSource m_TokenSource;

	protected override void Awake()
	{
		base.Awake();
		
		m_SignalBus.Subscribe<ScoreSignal>(RegisterScore);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_SignalBus.Unsubscribe<ScoreSignal>(RegisterScore);
	}

	void RegisterScore(ScoreSignal _Signal)
	{
		ProcessScore(_Signal.Score);
	}

	async void ProcessScore(long _Score)
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			await ScoreAsync(_Score, token);
		}
		catch (TaskCanceledException) { }
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	Task ScoreAsync(long _Score, CancellationToken _Token = default)
	{
		long source = m_Score;
		long target = _Score;
		
		if (source >= target || _Token.IsCancellationRequested)
		{
			m_Score            = target;
			m_ScoreLabel.Value = m_Score;
			return Task.CompletedTask;
		}
		
		_Token.Register(
			() =>
			{
				m_Score            = target;
				m_ScoreLabel.Value = m_Score;
			}
		);
		
		long delta = target - source;
		
		return UnityTask.Phase(
			_Phase =>
			{
				m_Score            = source + (long)(delta * _Phase);
				m_ScoreLabel.Value = m_Score;
			},
			m_Duration,
			_Token
		);
	}
}