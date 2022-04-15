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

	[Inject] SignalBus    m_SignalBus;
	[Inject] ScoreManager m_ScoreManager;

	CancellationTokenSource m_TokenSource;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_SignalBus.Subscribe<TapSuccessSignal>(RegisterTapSuccess);
		m_SignalBus.Subscribe<TapFailSignal>(RegisterTapFail);
		
		m_SignalBus.Subscribe<DoubleSuccessSignal>(RegisterDoubleSuccess);
		m_SignalBus.Subscribe<DoubleFailSignal>(RegisterDoubleFail);
		
		m_SignalBus.Subscribe<HoldSuccessSignal>(RegisterHoldSuccess);
		m_SignalBus.Subscribe<HoldMissSignal>(RegisterHoldMiss);
		m_SignalBus.Subscribe<HoldHitSignal>(RegisterHoldHit);
		m_SignalBus.Subscribe<HoldFailSignal>(RegisterHoldFail);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_SignalBus.Unsubscribe<TapSuccessSignal>(RegisterTapSuccess);
		m_SignalBus.Unsubscribe<TapFailSignal>(RegisterTapFail);
		
		m_SignalBus.Unsubscribe<DoubleSuccessSignal>(RegisterDoubleSuccess);
		m_SignalBus.Unsubscribe<DoubleFailSignal>(RegisterDoubleFail);
		
		m_SignalBus.Unsubscribe<HoldSuccessSignal>(RegisterHoldSuccess);
		m_SignalBus.Unsubscribe<HoldMissSignal>(RegisterHoldMiss);
		m_SignalBus.Unsubscribe<HoldHitSignal>(RegisterHoldHit);
		m_SignalBus.Unsubscribe<HoldFailSignal>(RegisterHoldFail);
	}

	void RegisterTapSuccess(TapSuccessSignal _Signal)
	{
		Process(_Signal.Progress);
	} 

	void RegisterTapFail(TapFailSignal _Signal)
	{
		Play(m_Fail);
	}

	void RegisterDoubleSuccess(DoubleSuccessSignal _Signal)
	{
		Process(_Signal.Progress);
	}

	void RegisterDoubleFail(DoubleFailSignal _Signal)
	{
		Play(m_Fail);
	}

	void RegisterHoldSuccess(HoldSuccessSignal _Signal)
	{
		Process(_Signal.MaxProgress - _Signal.MinProgress);
	}

	void RegisterHoldMiss(HoldMissSignal _Signal)
	{
		Play(m_Bad);
	}

	void RegisterHoldHit(HoldHitSignal _Signal)
	{
		Process(_Signal.Progress);
	}

	void RegisterHoldFail(HoldFailSignal _Signal)
	{
		Play(m_Fail);
	}

	void Process(float _Progress)
	{
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