using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.SetupMenu)]
public class UISetupMenu : UIMenu
{
	[SerializeField] UIGroup            m_IntroGroup;
	[SerializeField] UIGroup            m_LatencyGroup;
	[SerializeField] UIGroup            m_CompleteGroup;
	[SerializeField] UILatencyIndicator m_LatencyIndicator;

	[Inject] MenuProcessor m_MenuProcessor;

	protected override void OnShowStarted()
	{
		m_IntroGroup.Hide(true);
		m_LatencyGroup.Hide(true);
		m_CompleteGroup.Hide(true);
	}

	protected override void OnShowFinished()
	{
		ShowIntro();
	}

	protected override void OnHideFinished()
	{
		m_MenuProcessor.RemoveMenu(MenuType.SetupMenu);
	}

	public async void ShowIntro()
	{
		await Task.WhenAll(
			m_IntroGroup.ShowAsync(),
			m_LatencyGroup.HideAsync(),
			m_CompleteGroup.HideAsync()
		);
	}

	public async void ShowLatency()
	{
		await Task.WhenAll(
			m_LatencyGroup.ShowAsync(),
			m_IntroGroup.HideAsync(),
			m_CompleteGroup.HideAsync()
		);
		
		m_LatencyIndicator.Process();
	}

	public async void ShowComplete()
	{
		m_LatencyIndicator.Complete();
		
		await Task.WhenAll(
			m_CompleteGroup.ShowAsync(),
			m_IntroGroup.HideAsync(),
			m_LatencyGroup.HideAsync()
		);
	}

	public async void Finish()
	{
		await m_MenuProcessor.Show(MenuType.LoginMenu);
		await m_MenuProcessor.Hide(MenuType.SetupMenu, true);
		
		UILoginMenu loginMenu = m_MenuProcessor.GetMenu<UILoginMenu>();
		
		await loginMenu.Login();
	}
}