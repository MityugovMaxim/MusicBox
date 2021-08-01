using System;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

public class AudioProcessor : MonoBehaviour, IInitializable, IDisposable
{
	[SerializeField] AudioMixerSnapshot m_HitSnapshot;
	[SerializeField] AudioMixerSnapshot m_MissSnapshot;

	SignalBus m_SignalBus;

	public void Restore()
	{
		m_HitSnapshot.TransitionTo(0);
	}

	void RegisterHit()
	{
		m_HitSnapshot.TransitionTo(0.1f);
	}

	void RegisterMiss()
	{
		m_MissSnapshot.TransitionTo(0.1f);
	}

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	public void Initialize()
	{
		m_SignalBus.Subscribe<HoldSuccessSignal>(RegisterHit);
		m_SignalBus.Subscribe<HoldHitSignal>(RegisterHit);
		m_SignalBus.Subscribe<TapSuccessSignal>(RegisterHit);
		m_SignalBus.Subscribe<DoubleSuccessSignal>(RegisterHit);
		
		m_SignalBus.Subscribe<HoldFailSignal>(RegisterMiss);
		m_SignalBus.Subscribe<HoldMissSignal>(RegisterMiss);
		m_SignalBus.Subscribe<TapFailSignal>(RegisterMiss);
		m_SignalBus.Subscribe<DoubleFailSignal>(RegisterMiss);
	}

	public void Dispose()
	{
		m_SignalBus.Unsubscribe<HoldSuccessSignal>(RegisterHit);
		m_SignalBus.Unsubscribe<HoldHitSignal>(RegisterHit);
		m_SignalBus.Unsubscribe<TapSuccessSignal>(RegisterHit);
		m_SignalBus.Unsubscribe<DoubleSuccessSignal>(RegisterHit);
		
		m_SignalBus.Unsubscribe<HoldFailSignal>(RegisterMiss);
		m_SignalBus.Unsubscribe<HoldMissSignal>(RegisterMiss);
		m_SignalBus.Unsubscribe<TapFailSignal>(RegisterMiss);
		m_SignalBus.Unsubscribe<DoubleFailSignal>(RegisterMiss);
	}
}
