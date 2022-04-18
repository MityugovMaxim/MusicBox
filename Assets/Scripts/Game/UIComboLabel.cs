using System;
using TMPro;
using UnityEngine;
using Zenject;

public class UIComboLabel : UIEntity, IInitializable, IDisposable
{
	[SerializeField] TMP_Text m_Label;

	SignalBus m_SignalBus;

	int m_Multiplier;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<ScoreSignal>(RegisterLevelCombo);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<ScoreSignal>(RegisterLevelCombo);
	}

	void RegisterLevelCombo(ScoreSignal _Signal)
	{
		SetMultiplier(_Signal.Multiplier);
	}

	void SetMultiplier(int _Multiplier)
	{
		m_Multiplier = Mathf.Max(1, _Multiplier);
		
		m_Label.text = $"×{m_Multiplier}";
	}
}