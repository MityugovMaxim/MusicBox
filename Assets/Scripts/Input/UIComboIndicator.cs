using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIComboIndicator : UIGroup
{
	[SerializeField] UIUnitLabel m_Label;
	[SerializeField] Graphic     m_Graphic;
	[SerializeField] Color       m_DefaultColor;
	[SerializeField] Color       m_PerfectColor;
	[SerializeField] Color       m_GoodColor;
	[SerializeField] Color       m_BadColor;
	[SerializeField] Color       m_FailColor;
	[SerializeField] float       m_SourcePosition;
	[SerializeField] float       m_TargetPosition;
	[SerializeField] float       m_Duration;

	[Inject] SignalBus m_SignalBus;

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

	async void RegisterScore(ScoreSignal _Signal)
	{
		Color color = m_DefaultColor;
		switch (_Signal.Grade)
		{
			case ScoreGrade.Perfect:
				color = m_PerfectColor;
				break;
			case ScoreGrade.Good:
				color = m_GoodColor;
				break;
			case ScoreGrade.Bad:
				color = m_BadColor;
				break;
			case ScoreGrade.Fail:
				color = m_FailColor;
				break;
		}
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		if (_Signal.Combo > 0)
			Show(true);
		
		try
		{
			if (m_Label.Value < _Signal.Combo)
			{
				m_Label.Value = _Signal.Combo;
				
				await Task.WhenAll(
					ColorAsync(color, m_DefaultColor, token),
					PositionAsync(m_SourcePosition, m_TargetPosition, token)
				);
			}
			else
			{
				m_Label.Value = _Signal.Combo;
				
				await ColorAsync(m_Graphic.color, color, token);
			}
		}
		catch (TaskCanceledException) { }
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		if (_Signal.Combo <= 0)
			Hide(true);
	}

	Task ColorAsync(Color _Source, Color _Target, CancellationToken _Token = default)
	{
		return UnityTask.Phase(
			_Phase =>
			{
				m_Graphic.color = EaseFunction.EaseOutQuad.Get(_Source, _Target, _Phase);
			},
			m_Duration,
			_Token
		);
	}

	Task PositionAsync(float _Source, float _Target, CancellationToken _Token = default)
	{
		Vector2 source = new Vector2(0, _Source);
		Vector2 target = new Vector2(0, _Target);
		
		return UnityTask.Phase(
			_Phase =>
			{
				m_Graphic.rectTransform.localPosition = EaseFunction.EaseOutBack.Get(source, target, _Phase);
			},
			m_Duration,
			_Token
		);
	}
}