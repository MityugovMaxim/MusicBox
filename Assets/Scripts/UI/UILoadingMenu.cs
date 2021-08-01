using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class UILoadingMenu : UIMenu, IInitializable, IDisposable
{
	SignalBus      m_SignalBus;
	LevelProcessor m_LevelProcessor;

	string m_LevelID;

	[Inject]
	public void Construct(
		SignalBus      _SignalBus,
		LevelProcessor _LevelProcessor
	)
	{
		m_SignalBus      = _SignalBus;
		m_LevelProcessor = _LevelProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
	}

	protected override void OnShowFinished()
	{
		m_LevelProcessor.Create(m_LevelID);
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
	}

	void RegisterLevelStart()
	{
		CloseAction = m_LevelProcessor.Play;
		
		StartCoroutine(DelayRoutine(0.5f, () => Hide()));
	}

	IEnumerator DelayRoutine(float _Delay, Action _Callback)
	{
		yield return new WaitForSeconds(_Delay);
		
		_Callback?.Invoke();
	}
}