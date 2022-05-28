using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.SetupMenu)]
public class UISetupMenu : UIMenu
{
	[SerializeField] UILatencyIndicator m_LatencyIndicator;

	[Inject] SignalBus        m_SignalBus;
	[Inject] MenuProcessor    m_MenuProcessor;
	[Inject] AmbientProcessor m_AmbientProcessor;

	TaskCompletionSource<bool> m_CompletionSource;

	protected override void OnShowStarted()
	{
		m_SignalBus.Subscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
		
		m_AmbientProcessor.Pause();
	}

	protected override void OnHideStarted()
	{
		m_SignalBus.Unsubscribe<AudioSourceChangedSignal>(RegisterAudioSourceChanged);
	}

	protected override void OnHideFinished()
	{
		m_MenuProcessor.RemoveMenu(MenuType.SetupMenu);
		
		m_AmbientProcessor.Resume();
	}

	public Task Process()
	{
		m_CompletionSource?.TrySetResult(true);
		
		m_CompletionSource = new TaskCompletionSource<bool>();
		
		m_LatencyIndicator.Process();
		
		return m_CompletionSource.Task;
	}

	public void Complete()
	{
		m_LatencyIndicator.Complete();
		
		m_CompletionSource.TrySetResult(true);
		
		m_CompletionSource = null;
	}

	void RegisterAudioSourceChanged()
	{
		m_LatencyIndicator.Process();
	}
}