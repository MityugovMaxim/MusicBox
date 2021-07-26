using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class ScoreProcessor : IInitializable, IDisposable
{
	readonly SignalBus m_SignalBus;

	readonly List<float> m_Success = new List<float>();
	readonly List<float> m_Fail    = new List<float>();

	public ScoreProcessor(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	public void Restore()
	{
		m_Success.Clear();
		m_Fail.Clear();
	}

	public float GetScore()
	{
		return m_Success.Sum() + m_Fail.Sum();
	}

	void RegisterHoldSuccess(HoldSuccess _Signal)
	{
		m_Success.Add(_Signal.Progress);
	}

	void RegisterTapSuccess(TapSuccess _Signal)
	{
		m_Success.Add(_Signal.Progress);
	}

	void RegisterDoubleSuccess(DoubleSuccess _Signal)
	{
		m_Success.Add(_Signal.Progress);
	}

	void RegisterHoldFail(HoldFail _Signal)
	{
		m_Fail.Add(_Signal.Progress);
	}

	void RegisterTapFail(TapFail _Signal)
	{
		m_Fail.Add(_Signal.Progress);
	}

	void RegisterDoubleFail(DoubleFail _Signal)
	{
		m_Fail.Add(_Signal.Progress);
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<HoldSuccess>(RegisterHoldSuccess);
		m_SignalBus.Subscribe<TapSuccess>(RegisterTapSuccess);
		m_SignalBus.Subscribe<DoubleSuccess>(RegisterDoubleSuccess);
		
		m_SignalBus.Subscribe<HoldFail>(RegisterHoldFail);
		m_SignalBus.Subscribe<TapFail>(RegisterTapFail);
		m_SignalBus.Subscribe<DoubleFail>(RegisterDoubleFail);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<HoldSuccess>(RegisterHoldSuccess);
		m_SignalBus.Unsubscribe<TapSuccess>(RegisterTapSuccess);
		m_SignalBus.Unsubscribe<DoubleSuccess>(RegisterDoubleSuccess);
		
		m_SignalBus.Unsubscribe<HoldFail>(RegisterHoldFail);
		m_SignalBus.Unsubscribe<TapFail>(RegisterTapFail);
		m_SignalBus.Unsubscribe<DoubleFail>(RegisterDoubleFail);
	}
}
