using System;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class HapticProcessor : IInitializable, IDisposable
{
	SignalBus m_SignalBus;
	Haptic    m_Haptic;

	[Inject]
	public void Construct(SignalBus _SignalBus, Haptic _Haptic)
	{
		m_SignalBus = _SignalBus;
		m_Haptic    = _Haptic;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<HoldSuccess>(RegisterSuccess);
		m_SignalBus.Subscribe<TapSuccess>(RegisterSuccess);
		m_SignalBus.Subscribe<DoubleSuccess>(RegisterSuccess);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<HoldSuccess>(RegisterSuccess);
		m_SignalBus.Unsubscribe<TapSuccess>(RegisterSuccess);
		m_SignalBus.Unsubscribe<DoubleSuccess>(RegisterSuccess);
	}

	void RegisterSuccess()
	{
		m_Haptic.Process(Haptic.Type.ImpactMedium);
	}
}
