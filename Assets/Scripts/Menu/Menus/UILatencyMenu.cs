using UnityEngine;
using Zenject;

[Menu(MenuType.LatencyMenu)]
public class UILatencyMenu : UISlideMenu
{
	[SerializeField] UILatencyIndicator m_LatencyIndicator;

	[Inject] SignalBus        m_SignalBus;
	[Inject] AmbientProcessor m_AmbientProcessor;

	public void Restore()
	{
		m_LatencyIndicator.Restore();
	}

	protected override void OnShowStarted()
	{
		m_SignalBus.Subscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
		
		m_AmbientProcessor.Pause();
		
		m_LatencyIndicator.Process();
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
	}

	protected override void OnHideFinished()
	{
		m_LatencyIndicator.Complete();
		
		m_AmbientProcessor.Resume();
	}

	void RegisterAudioSourceChanged()
	{
		m_LatencyIndicator.Process();
	}
}