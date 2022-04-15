using UnityEngine;
using Zenject;

public class UIComboIndicator : UIGroup
{
	[SerializeField] UIUnitLabel m_Label;

	[Inject] SignalBus m_SignalBus;

	protected override void Awake()
	{
		base.Awake();
		
		m_SignalBus.Subscribe<ScoreSignal>(RegisterScore);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_SignalBus.Unsubscribe<ScoreSignal>(RegisterScore);
	}

	void RegisterScore(ScoreSignal _Signal)
	{
		m_Label.Value = _Signal.Combo;
		
		if (_Signal.Combo > 0)
			Show();
		else
			Hide();
	}
}