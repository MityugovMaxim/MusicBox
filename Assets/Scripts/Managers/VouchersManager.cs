using System;
using Zenject;

public class VouchersManager : IInitializable, IDisposable
{
	[Inject] SignalBus        m_SignalBus;
	[Inject] ProfileProcessor m_ProfileProcessor;

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<ProfileDataUpdateSignal>(Refresh);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<ProfileDataUpdateSignal>(Refresh);
	}

	void Refresh()
	{
	}
}
