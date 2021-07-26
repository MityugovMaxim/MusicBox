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
		m_SignalBus.Subscribe<HoldSuccess>(RegisterHit);
		m_SignalBus.Subscribe<HoldHit>(RegisterHit);
		m_SignalBus.Subscribe<TapSuccess>(RegisterHit);
		m_SignalBus.Subscribe<DoubleSuccess>(RegisterHit);
		
		m_SignalBus.Subscribe<HoldFail>(RegisterMiss);
		m_SignalBus.Subscribe<HoldMiss>(RegisterMiss);
		m_SignalBus.Subscribe<TapFail>(RegisterMiss);
		m_SignalBus.Subscribe<DoubleFail>(RegisterMiss);
	}

	public void Dispose()
	{
		m_SignalBus.Unsubscribe<HoldSuccess>(RegisterHit);
		m_SignalBus.Unsubscribe<HoldHit>(RegisterHit);
		m_SignalBus.Unsubscribe<TapSuccess>(RegisterHit);
		m_SignalBus.Unsubscribe<DoubleSuccess>(RegisterHit);
		
		m_SignalBus.Unsubscribe<HoldFail>(RegisterMiss);
		m_SignalBus.Unsubscribe<HoldMiss>(RegisterMiss);
		m_SignalBus.Unsubscribe<TapFail>(RegisterMiss);
		m_SignalBus.Unsubscribe<DoubleFail>(RegisterMiss);
	}
}
