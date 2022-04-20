using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.SetupMenu)]
public class UISetupMenu : UIMenu
{
	[SerializeField] UILatencyIndicator m_LatencyIndicator;

	[Inject] MenuProcessor m_MenuProcessor;

	TaskCompletionSource<bool> m_CompletionSource;

	protected override void OnHideFinished()
	{
		m_MenuProcessor.RemoveMenu(MenuType.SetupMenu);
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
}