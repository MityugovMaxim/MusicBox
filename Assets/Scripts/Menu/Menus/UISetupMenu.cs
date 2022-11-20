using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.SetupMenu)]
public class UISetupMenu : UIMenu
{
	[SerializeField] UILatencyIndicator m_LatencyIndicator;

	[Inject] AudioManager     m_AudioManager;
	[Inject] MenuProcessor    m_MenuProcessor;
	[Inject] AmbientManager m_AmbientManager;

	TaskCompletionSource<bool> m_CompletionSource;

	protected override void OnShowStarted()
	{
		m_AmbientManager.Pause();
		
		m_AudioManager.OnSourceChange += OnSourceChange;
	}

	protected override void OnHideStarted()
	{
		m_AudioManager.OnSourceChange -= OnSourceChange;
	}

	protected override void OnHideFinished()
	{
		m_MenuProcessor.RemoveMenu(MenuType.SetupMenu);
		
		m_AmbientManager.Play();
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

	void OnSourceChange()
	{
		m_LatencyIndicator.Process();
	}
}
