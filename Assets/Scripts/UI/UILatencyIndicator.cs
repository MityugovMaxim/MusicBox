using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

public class UILatencyIndicator : UIEntity
{
	const float MANUAL_LATENCY_STEP = 0.005f;

	[SerializeField] TMP_Text      m_OutputLabel;
	[SerializeField] AudioSource   m_AudioSource;
	[SerializeField] CanvasGroup   m_Flash;
	[SerializeField] CanvasGroup   m_Group;
	[SerializeField] RectTransform m_Indicator;
	[SerializeField] RectTransform m_Handle;
	[SerializeField] float         m_MinLimit = -1.0f;
	[SerializeField] float         m_MaxLimit = 1.0f;

	[Inject] AudioManager m_AudioManager;

	CancellationTokenSource m_TokenSource;

	float m_Latency;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessSettings();
		
		ProcessLatency();
	}

	public void Process()
	{
		StopProcess();
		
		ProcessSettings();
		
		ProcessLatency();
		
		StartProcess();
	}

	public void Complete()
	{
		StopProcess();
		
		m_AudioManager.SetLatency(m_Latency);
		
		// TODO: Uncomment after moving from splash
		// m_StatisticProcessor.LogAudioLatencyState(
		// 	m_AudioManager.GetAudioOutputName(),
		// 	m_AudioManager.GetAudioOutputID(),
		// 	m_AudioManager.GetAudioOutputType(),
		// 	m_AudioManager.GetLatency()
		// );
	}

	static string GetOutputIcon(AudioOutputType _OutputType)
	{
		switch (_OutputType)
		{
			case AudioOutputType.BuiltIn:
				return "<sprite name=speaker_icon>";
			case AudioOutputType.Headphones:
				return "<sprite name=headphones_icon>";
			case AudioOutputType.Bluetooth:
				return "<sprite name=bluetooth_logo>";
			default:
				return string.Empty;
		}
	}

	public void Increase()
	{
		m_Latency += MANUAL_LATENCY_STEP;
		
		ProcessLatency();
	}

	public void Decrease()
	{
		m_Latency -= MANUAL_LATENCY_STEP;
		
		ProcessLatency();
	}

	void ProcessSettings()
	{
		string          outputName = m_AudioManager.GetAudioOutputName();
		AudioOutputType outputType = m_AudioManager.GetAudioOutputType();
		string          outputIcon = GetOutputIcon(outputType);
		
		m_Latency = m_AudioManager.GetLatency();
		
		m_OutputLabel.text = $"{outputIcon}{outputName}";
	}

	void ProcessLatency()
	{
		m_Latency = Mathf.Clamp(m_Latency, m_MinLimit, m_MaxLimit);
		
		float phase = Mathf.InverseLerp(m_MaxLimit, m_MinLimit, m_Latency);
		
		Vector2 anchor = new Vector2(0.5f, phase);
		
		m_Handle.anchorMin = anchor;
		m_Handle.anchorMax = anchor;
	}

	async void StartProcess()
	{
		StopProcess();
		
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

	void StopProcess()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_Group.alpha = 0;
		m_AudioSource.Stop();
	}

	async Task ShowAsync(CancellationToken _Token = default)
	{
		await UnityTask.Phase(
			_Phase => m_Group.alpha = _Phase,
			0.1f,
			_Token
		);
	}

	async Task HideAsync(CancellationToken _Token = default)
	{
		await UnityTask.Phase(
			_Phase => m_Group.alpha = 1 - _Phase,
			0.1f,
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
			m_Latency - m_MinLimit,
			0.15f,
			_Token
		);
	}

	async Task BeatAsync(CancellationToken _Token = default)
	{
		float delay = -m_MinLimit;
		
		m_AudioSource.Stop();
		m_AudioSource.PlayScheduled(AudioSettings.dspTime + delay);
		
		await UnityTask.Delay(m_AudioSource.clip.length, _Token);
	}
}