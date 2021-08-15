using System;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class HapticProcessor : IInitializable, IDisposable
{
	SignalBus m_SignalBus;
	Haptic    m_Haptic;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
		m_Haptic    = Haptic.Create();
	}

	public void Process(Haptic.Type _HapticType)
	{
		m_Haptic.Process(_HapticType);
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
		m_Haptic.Process(Haptic.Type.ImpactHeavy);
	}

	void ImpactMedium()
	{
		m_Haptic.Process(Haptic.Type.ImpactMedium);
	}

	void ImpactLight()
	{
		m_Haptic.Process(Haptic.Type.ImpactLight);
	}
}
