using System;
using Zenject;

public class UIGameMenu : UIMenu, IInitializable, IDisposable
{
	SignalBus m_SignalBus;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelExitSignal>(RegisterLevelExit);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelExitSignal>(RegisterLevelExit);
	}

	void RegisterLevelStart()
	{
		Show(true);
	}

	void RegisterLevelExit()
	{
		Hide();
	}
}