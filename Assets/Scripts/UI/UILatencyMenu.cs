using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using OutputType = AudioManager.OutputType;

[Menu(MenuType.LatencyMenu)]
public class UILatencyMenu : UISlideMenu, IInitializable, IDisposable
{
	const int MANUAL_LATENCY_STEP = 25;

	[SerializeField] UILevelBackground m_Background;
	[SerializeField] TMP_Text          m_OutputNameLabel;
	[SerializeField] TMP_Text          m_ManualLatencyLabel;
	[SerializeField] AudioSource       m_AudioSource;
	[SerializeField] CanvasGroup       m_IndicatorGroup;
	[SerializeField] RectTransform     m_Indicator;
	[SerializeField] RectTransform     m_Zone;
	[SerializeField] int               m_MinLimit = -1000;
	[SerializeField] int               m_MaxLimit = 1000;
	[SerializeField] Button            m_IncreaseButton;
	[SerializeField] Button            m_DecreaseButton;

	SignalBus          m_SignalBus;
	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;

	int         m_ManualLatency;
	IEnumerator m_LatencyRoutine;

	[Inject]
	public void Construct(
		SignalBus          _SignalBus,
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_SignalBus          = _SignalBus;
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
		
		m_IncreaseButton.onClick.AddListener(Increase);
		m_DecreaseButton.onClick.AddListener(Decrease);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
		
		m_IncreaseButton.onClick.RemoveListener(Increase);
		m_DecreaseButton.onClick.RemoveListener(Decrease);
	}

	void RegisterAudioSourceChanged()
	{
		Reload();
	}

	protected override void OnShowStarted()
	{
		if (m_LatencyRoutine != null)
			StopCoroutine(m_LatencyRoutine);
		
		Reload();
		
		m_LatencyRoutine = LatencyRoutine();
		
		StartCoroutine(m_LatencyRoutine);
	}

	protected override void OnHideFinished()
	{
		float manualLatency = m_ManualLatency * 0.001f;
		
		AudioManager.SetManualLatency(manualLatency);
		
		m_StatisticProcessor.LogLatencyMenuState(
			AudioManager.GetAudioOutputName(),
			AudioManager.GetAudioOutputUID(),
			AudioManager.GetAudioOutputType().ToString(),
			AudioManager.GetManualLatency()
		);
		
		if (m_LatencyRoutine != null)
			StopCoroutine(m_LatencyRoutine);
		
		m_AudioSource.Stop();
	}

	public void Setup(string _LevelID)
	{
		m_Background.Setup(_LevelID);
	}

	void Reload()
	{
		float      manualLatency = AudioManager.GetManualLatency();
		string     outputName    = AudioManager.GetAudioOutputName();
		OutputType outputType    = AudioManager.GetAudioOutputType();
		string     outputIcon    = GetOutputIcon(outputType);
		
		m_ManualLatency        = (int)(manualLatency * 1000);
		m_OutputNameLabel.text = $"{outputIcon}{outputName}";
		
		ProcessManualLatency();
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

	void Increase()
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		m_ManualLatency += MANUAL_LATENCY_STEP;
		
		ProcessManualLatency();
	}

	void Decrease()
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		m_ManualLatency -= MANUAL_LATENCY_STEP;
		
		ProcessManualLatency();
	}

	IEnumerator LatencyRoutine()
	{
		float delay = 1 - AudioManager.GetHardwareLatency();
		
		while (true)
		{
			m_AudioSource.PlayScheduled(AudioSettings.dspTime + delay);
			
			const float source = 1;
			const float target = 0;
			
			Vector2 anchorMin = m_Indicator.anchorMin;
			Vector2 anchorMax = m_Indicator.anchorMax;
			
			anchorMin.y = source;
			anchorMax.y = source;
			
			m_Indicator.anchorMin = anchorMin;
			m_Indicator.anchorMax = anchorMax;
			
			const float duration = 2;
			
			float time = 0;
			while (time < duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				float phase = time / duration;
				
				float sourceAlpha = MathUtility.Remap01Clamped(phase, 0, 0.05f);
				float targetAlpha = MathUtility.Remap01Clamped(phase, 1, 0.95f);
				
				float anchor = Mathf.Lerp(source, target, phase);
				
				m_IndicatorGroup.alpha = sourceAlpha * targetAlpha;
				
				anchorMin.y = anchor;
				anchorMax.y = anchor;
				
				m_Indicator.anchorMin = anchorMin;
				m_Indicator.anchorMax = anchorMax;
			}
			
			anchorMin.y = target;
			anchorMax.y = target;
			
			m_Indicator.anchorMin = anchorMin;
			m_Indicator.anchorMax = anchorMax;
		}
	}

	void ProcessManualLatency()
	{
		m_ManualLatency = Mathf.Clamp(m_ManualLatency, m_MinLimit, m_MaxLimit);
		
		m_ManualLatencyLabel.text = $"{m_ManualLatency} ms";
		
		Vector2 anchorMin = m_Zone.anchorMin;
		Vector2 anchorMax = m_Zone.anchorMax;
		
		float phase = Mathf.InverseLerp(m_MaxLimit, m_MinLimit, m_ManualLatency);
		
		anchorMin.y = phase;
		anchorMax.y = phase;
		
		m_Zone.anchorMin = anchorMin;
		m_Zone.anchorMax = anchorMax;
	}
}