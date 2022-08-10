using UnityEngine;
using Zenject;

[Menu(MenuType.LatencyMenu)]
public class UILatencyMenu : UISlideMenu
{
	[SerializeField] UILatencyIndicator m_LatencyIndicator;

	[Inject] SignalBus        m_SignalBus;
	[Inject] AmbientProcessor m_AmbientProcessor;

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		m_SignalBus.Subscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
		
		m_AmbientProcessor.Pause();
		
		m_LatencyIndicator.Process();
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		
		m_LatencyIndicator.Complete();
		
		if (m_SignalBus == null)
			return;
		
		m_SignalBus.Unsubscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
	}

	protected override void OnHideFinished()
	{
		base.OnHideFinished();
		
		m_AmbientProcessor.Resume();
	}

	protected override bool OnEscape()
	{
		Hide();
		
		return true;
	}

	void RegisterAudioSourceChanged()
	{
		m_LatencyIndicator.Process();
	}
}