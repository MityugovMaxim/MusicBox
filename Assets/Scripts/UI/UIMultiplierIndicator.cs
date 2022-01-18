using System;
using UnityEngine;
using Zenject;

public class UIMultiplierIndicator : UIEntity, IInitializable, IDisposable
{
	[SerializeField] UIMultiplierProgress m_MultiplierProgress;
	[SerializeField] UIMultiplierLabel    m_MultiplierLabel;

	SignalBus m_SignalBus;

	int   m_Multiplier;
	float m_Progress;

	protected override void Awake()
	{
		base.Awake();
		
		ProcessMultiplier(1, true);
		ProcessProgress(0, true);
	}

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			ProcessProgress(m_Progress + 0.05f);
			ProcessMultiplier(m_Multiplier);
		}
		if (Input.GetKeyDown(KeyCode.G))
		{
			ProcessProgress(0);
			ProcessMultiplier(m_Multiplier + 1);
		}
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Subscribe<LevelComboSignal>(RegisterCombo);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Unsubscribe<LevelComboSignal>(RegisterCombo);
	}

	void RegisterLevelStart()
	{
		ProcessMultiplier(1, true);
		
		ProcessProgress(0, true);
	}

	void RegisterLevelRestart()
	{
		ProcessMultiplier(1, true);
		
		ProcessProgress(0, true);
	}

	void RegisterCombo(LevelComboSignal _Signal)
	{
		ProcessMultiplier(_Signal.Multiplier);
		
		ProcessProgress(_Signal.Progress);
	}

	void ProcessMultiplier(int _Multiplier, bool _Instant = false)
	{
		m_MultiplierLabel.Play(_Multiplier, _Instant || m_Multiplier >= _Multiplier);
		
		if (m_Multiplier < _Multiplier)
		{
			m_Progress = 0;
			m_MultiplierProgress.Progress(0, true);
			m_MultiplierProgress.Play(_Instant);
		}
		
		m_Multiplier = _Multiplier;
	}

	void ProcessProgress(float _Progress, bool _Instant = false)
	{
		if (m_Progress > _Progress)
			m_MultiplierProgress.Progress(0, true);
		
		m_Progress = _Progress;
		
		m_MultiplierProgress.Progress(m_Progress, _Instant);
	}
}