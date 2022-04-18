using System;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

public class UIInputIndicator : UIEntity
{
	[SerializeField] float     m_Duration;
	[SerializeField] UIRounded m_Perfect;
	[SerializeField] UIRounded m_Good;
	[SerializeField] UIRounded m_Bad;
	[SerializeField] UIRounded m_Fail;

	[Inject] SignalBus m_SignalBus;

	CancellationTokenSource m_TokenSource;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_SignalBus.Subscribe<ScoreSignal>(RegisterScore);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_SignalBus.Unsubscribe<ScoreSignal>(RegisterScore);
	}

	void RegisterScore(ScoreSignal _Signal)
	{
		switch (_Signal.Grade)
		{
			case ScoreGrade.Perfect:
				Play(m_Perfect);
				break;
			case ScoreGrade.Good:
				Play(m_Good);
				break;
			case ScoreGrade.Bad:
				Play(m_Bad);
				break;
			case ScoreGrade.Fail:
				Play(m_Fail);
				break;
		}
	}

	async void Play(UIRounded _Outline)
	{
		if (_Outline == null)
			return;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		Color source = new Color(1, 1, 1, 1);
		Color target = new Color(1, 1, 1, 0);
		
		_Outline.rectTransform.SetAsLastSibling();
		_Outline.gameObject.SetActive(true);
		_Outline.color = source;
		
		try
		{
			await UnityTask.Phase(
				_Phase => _Outline.color = Color.Lerp(source, target, _Phase),
				m_Duration,
				token
			);
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		
		_Outline.gameObject.SetActive(false);
		_Outline.color = target;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}