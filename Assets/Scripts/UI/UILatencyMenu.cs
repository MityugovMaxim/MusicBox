using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;
using OutputType = AudioManager.OutputType;

[Menu(MenuType.LatencyMenu)]
public class UILatencyMenu : UISlideMenu, IInitializable, IDisposable
{
	const float MANUAL_LATENCY_STEP = 0.005f;

	[SerializeField] TMP_Text      m_OutputLabel;
	[SerializeField] TMP_Text      m_LatencyLabel;
	[SerializeField] AudioSource   m_AudioSource;
	[SerializeField] CanvasGroup   m_Flash;
	[SerializeField] CanvasGroup   m_Group;
	[SerializeField] RectTransform m_Indicator;
	[SerializeField] RectTransform m_Handle;
	[SerializeField] float         m_MinLimit = -1.0f;
	[SerializeField] float         m_MaxLimit = 1.0f;

	[Inject] SignalBus          m_SignalBus;
	[Inject] AmbientProcessor   m_AmbientProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	float m_HardwareLatency;
	float m_ManualLatency;

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
	}

	void RegisterAudioSourceChanged()
	{
		Reload();
	}

	protected override void OnShowStarted()
	{
		m_AmbientProcessor.Pause();
		
		Reload();
		
		Process();
	}

	protected override void OnHideFinished()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_AmbientProcessor.Resume();
		
		m_Group.alpha = 0;
		m_AudioSource.Stop();
		
		AudioManager.SetManualLatency(m_ManualLatency);
		
		m_StatisticProcessor.LogLatencyMenuState(
			AudioManager.GetAudioOutputName(),
			AudioManager.GetAudioOutputUID(),
			AudioManager.GetAudioOutputType().ToString(),
			AudioManager.GetManualLatency()
		);
	}

	void Reload()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_ManualLatency   = AudioManager.GetManualLatency();
		m_HardwareLatency = AudioManager.GetHardwareLatency();
		
		string     outputName = AudioManager.GetAudioOutputName();
		OutputType outputType = AudioManager.GetAudioOutputType();
		string     outputIcon = GetOutputIcon(outputType);
		
		m_OutputLabel.text = $"{outputIcon}{outputName}";
		
		ProcessLatency();
	}

	static string GetOutputIcon(OutputType _OutputType)
	{
		switch (_OutputType)
		{
			case OutputType.BuiltIn:
				return "<sprite name=speaker_icon>";
			case OutputType.Headphones:
				return "<sprite name=headphones_icon>";
			case OutputType.Bluetooth:
				return "<sprite name=bluetooth_logo>";
			default:
				return string.Empty;
		}
	}

	public void Increase()
	{
		m_ManualLatency += MANUAL_LATENCY_STEP;
		
		ProcessLatency();
	}

	public void Decrease()
	{
		m_ManualLatency -= MANUAL_LATENCY_STEP;
		
		ProcessLatency();
	}

	void ProcessLatency()
	{
		m_ManualLatency = Mathf.Clamp(m_ManualLatency, m_MinLimit, m_MaxLimit);
		
		m_LatencyLabel.text = GetLocalization("COMMON_MILLISECONDS", Mathf.RoundToInt(m_ManualLatency * 1000));
		
		float phase = Mathf.InverseLerp(m_MaxLimit, m_MinLimit, m_ManualLatency);
		
		Vector2 anchor = new Vector2(0.5f, phase);
		
		m_Handle.anchorMin = anchor;
		m_Handle.anchorMax = anchor;
	}

	CancellationTokenSource m_TokenSource;

	async void Process()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		token.Register(
			() =>
			{
				m_Group.alpha = 0;
				m_AudioSource.Stop();
			}
		);
		
		try
		{
			while (!token.IsCancellationRequested)
			{
				await ShowAsync(token);
				
				await Task.WhenAll(
					BeatAsync(token),
					MoveAsync(token),
					FlashAsync(token)
				);
				
				await UnityTask.Yield(token);
				
				await HideAsync(token);
			}
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	async Task ShowAsync(CancellationToken _Token = default)
	{
		await UnityTask.Phase(
			_Phase => m_Group.alpha = _Phase,
			0.2f,
			_Token
		);
	}

	async Task HideAsync(CancellationToken _Token = default)
	{
		await UnityTask.Phase(
			_Phase => m_Group.alpha = 1 - _Phase,
			0.2f,
			_Token
		);
		
		m_Indicator.anchorMin = new Vector2(0.5f, 1);
		m_Indicator.anchorMax = new Vector2(0.5f, 1);
	}

	async Task MoveAsync(CancellationToken _Token = default)
	{
		await UnityTask.Phase(
			_Phase =>
			{
				Vector2 anchor = new Vector2(0.5f, 1 - _Phase);
				m_Indicator.anchorMin = anchor;
				m_Indicator.anchorMax = anchor;
			},
			m_MaxLimit - m_MinLimit,
			_Token
		);
	}

	async Task FlashAsync(CancellationToken _Token = default)
	{
		await UnityTask.Phase(
			_Phase => m_Flash.alpha = 1 - _Phase,
			m_HardwareLatency + m_ManualLatency - m_MinLimit,
			0.15f,
			_Token
		);
	}

	async Task BeatAsync(CancellationToken _Token = default)
	{
		float delay = m_HardwareLatency - m_MinLimit;
		
		m_AudioSource.Stop();
		m_AudioSource.PlayScheduled(AudioSettings.dspTime + delay);
		
		await UnityTask.Delay(m_AudioSource.clip.length, _Token);
	}
}