using System;
using UnityEngine;
using Zenject;

public class UIComboIndicator : UIGroup, IInitializable, IDisposable
{
	[SerializeField] UIUnitLabel m_Label;

	SignalBus m_SignalBus;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<SongComboSignal>(RegisterLevelCombo);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<SongComboSignal>(RegisterLevelCombo);
	}

	void RegisterLevelCombo(SongComboSignal _Signal)
	{
		m_Label.Value = _Signal.Combo;
		
		if (_Signal.Combo > 0)
			Show();
		else
			Hide();
	}

	void Restore()
	{
		Hide(true);
		
		m_Label.Value = 0;
	}
}