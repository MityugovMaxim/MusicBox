using System;
using UnityEngine;
using Zenject;

[Menu(MenuType.LatencyMenu)]
public class UILatencyMenu : UISlideMenu, IInitializable, IDisposable
{
	[SerializeField] UILatencyIndicator m_LatencyIndicator;

	[Inject] SignalBus        m_SignalBus;
	[Inject] AmbientProcessor m_AmbientProcessor;

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
	}

	void RegisterAudioSourceChanged()
	{
		m_LatencyIndicator.Process();
	}

	protected override void OnShowStarted()
	{
		m_AmbientProcessor.Pause();
		
		m_LatencyIndicator.Process();
	}

	protected override void OnHideFinished()
	{
		m_LatencyIndicator.Complete();
		
		m_AmbientProcessor.Resume();
	}
}