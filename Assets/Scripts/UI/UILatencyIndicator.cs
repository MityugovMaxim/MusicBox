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
	[SerializeField] UIUnitLabel   m_LatencyLabel;
	[SerializeField] float         m_MinLimit = -1.0f;
	[SerializeField] float         m_MaxLimit = 1.0f;
	[SerializeField] float         m_Duration = 2;

	[Inject] AudioManager    m_AudioManager;
	[Inject] ConfigProcessor m_ConfigProcessor;

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

	public void Sync()
	{
		Vector2 anchorMin = m_Indicator.anchorMin;
		Vector2 anchorMax = m_Indicator.anchorMax;
		
		float phase = (anchorMin.y + anchorMax.y) * 0.5f;
		
		m_Latency = Mathf.Lerp(m_MaxLimit, m_MinLimit, phase);
		
		m_Latency = Mathf.RoundToInt(m_Latency / MANUAL_LATENCY_STEP) * MANUAL_LATENCY_STEP;
		
		ProcessLatency();
	}

	public void Restore()
	{
		m_Latency = 0;
		
		ProcessLatency();
	}

	void ProcessSettings()
	{
		string          outputName = m_AudioManager.GetAudioOutputName();
		AudioOutputType outputType = m_AudioManager.GetAudioOutputType();
		string          outputIcon = GetOutputIcon(outputType);
		
		if (m_AudioManager.HasSettings())
			m_Latency = m_AudioManager.GetLatency();
		else if (m_AudioManager.GetAudioOutputType() == AudioOutputType.Bluetooth)
			m_Latency = m_ConfigProcessor.BluetoothLatency;
		else
			m_Latency = 0;
		
		m_OutputLabel.text = $"{outputIcon}{outputName}";
	}

	void ProcessLatency()
	{
		m_Latency = Mathf.Clamp(m_Latency, m_MinLimit, m_MaxLimit);
		
		m_LatencyLabel.Value = Mathf.RoundToInt(m_Latency * 1000);
		
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
				if (m_Group != null)
					m_Group.alpha = 0;
				
				if (m_AudioSource != null)
					m_AudioSource.Stop();
			}
		);
		
		try
		{
			while (!token.IsCancellationRequested)
			{
				m_Indicator.anchorMin = new Vector2(0.5f, 1);
				m_Indicator.anchorMax = new Vector2(0.5f, 1);
				
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
		catch (TaskCanceledException)
		{
			return;
		}
		catch (OperationCanceledException)
		{
			return;
		}
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

	Task ShowAsync(CancellationToken _Token = default)
	{
		_Token.ThrowIfCancellationRequested();
		
		if (m_Group == null)
			return Task.CompletedTask;
		
		return UnityTask.Phase(
			_Phase => m_Group.alpha = _Phase,
			0.1f,
			_Token
		);
	}

	Task HideAsync(CancellationToken _Token = default)
	{
		_Token.ThrowIfCancellationRequested();
		
		if (m_Group == null)
			return Task.CompletedTask;
		
		return UnityTask.Phase(
			_Phase => m_Group.alpha = 1 - _Phase,
			0.1f,
			_Token
		);
	}

	Task MoveAsync(CancellationToken _Token = default)
	{
		_Token.ThrowIfCancellationRequested();
		
		if (m_Indicator == null)
			return Task.CompletedTask;
		
		return UnityTask.Phase(
			_Phase =>
			{
				Vector2 anchor = new Vector2(0.5f, 1 - _Phase);
				m_Indicator.anchorMin = anchor;
				m_Indicator.anchorMax = anchor;
			},
			m_Duration,
			_Token
		);
	}

	Task FlashAsync(CancellationToken _Token = default)
	{
		_Token.ThrowIfCancellationRequested();
		
		if (m_Flash == null)
			return Task.CompletedTask;
		
		float pivot = Mathf.Abs(m_MinLimit) / (m_MaxLimit - m_MinLimit);
		float scale = m_Duration / (m_MaxLimit - m_MinLimit);
		
		float delay = m_Duration * pivot + m_Latency * scale;
		
		return UnityTask.Phase(
			_Phase => m_Flash.alpha = 1 - _Phase,
			delay,
			0.15f,
			_Token
		);
	}

	Task BeatAsync(CancellationToken _Token = default)
	{
		_Token.ThrowIfCancellationRequested();
		
		if (m_AudioSource == null)
			return Task.CompletedTask;
		
		float pivot = Mathf.Abs(m_MinLimit) / (m_MaxLimit - m_MinLimit);
		
		float delay = m_Duration * pivot;
		
		m_AudioSource.Stop();
		m_AudioSource.PlayScheduled(AudioSettings.dspTime + delay);
		
		return UnityTask.Delay(m_AudioSource.clip.length, _Token);
	}
}