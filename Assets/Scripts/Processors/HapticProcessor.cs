using System;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class HapticProcessor : IInitializable, IDisposable
{
	const string HAPTIC_ENABLED_KEY = "HAPTIC_ENABLED";

	public bool HapticSupported => m_Haptic.SupportsHaptic;

	public bool HapticEnabled
	{
		get => m_HapticEnabled;
		set
		{
			if (m_HapticEnabled == value)
				return;
			
			m_HapticEnabled = value;
			
			PlayerPrefs.SetInt(HAPTIC_ENABLED_KEY, m_HapticEnabled ? 1 : 0);
		}
	}

	SignalBus m_SignalBus;
	Haptic    m_Haptic;
	bool      m_HapticEnabled;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus     = _SignalBus;
		m_Haptic        = Haptic.Create();
		m_HapticEnabled = PlayerPrefs.GetInt(HAPTIC_ENABLED_KEY, 1) > 0;
	}

	public void Process(Haptic.Type _HapticType)
	{
		if (m_HapticEnabled)
			m_Haptic.Process(_HapticType);
	}

	public async void Play(Haptic.Type _HapticType, int _Frequency, float _Duration)
	{
		await UnityTask.Tick(() => Process(_HapticType), _Frequency, _Duration);
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<DoubleSuccessSignal>(ImpactHeavy);
		m_SignalBus.Subscribe<HoldSuccessSignal>(ImpactMedium);
		m_SignalBus.Subscribe<TapSuccessSignal>(ImpactMedium);
		m_SignalBus.Subscribe<HoldHitSignal>(ImpactLight);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<DoubleSuccessSignal>(ImpactHeavy);
		m_SignalBus.Unsubscribe<HoldSuccessSignal>(ImpactMedium);
		m_SignalBus.Unsubscribe<TapSuccessSignal>(ImpactMedium);
		m_SignalBus.Unsubscribe<HoldHitSignal>(ImpactLight);
	}

	void ImpactHeavy()
	{
		Process(Haptic.Type.ImpactHeavy);
	}

	void ImpactMedium()
	{
		Process(Haptic.Type.ImpactMedium);
	}

	void ImpactLight()
	{
		Process(Haptic.Type.ImpactLight);
	}
}
