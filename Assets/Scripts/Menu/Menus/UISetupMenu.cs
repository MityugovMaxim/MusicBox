using UnityEngine;
using Zenject;

[Menu(MenuType.SetupMenu)]
public class UISetupMenu : UIMenu
{
	[SerializeField] UILatencyIndicator m_LatencyIndicator;

	[Inject] MenuProcessor m_MenuProcessor;

	protected override void OnHideFinished()
	{
		m_MenuProcessor.RemoveMenu(MenuType.SetupMenu);
	}

	public async void Complete()
	{
		m_LatencyIndicator.Complete();
		
		await m_MenuProcessor.Show(MenuType.LoginMenu);
		await m_MenuProcessor.Hide(MenuType.SetupMenu, true);
		
		UILoginMenu loginMenu = m_MenuProcessor.GetMenu<UILoginMenu>();
		
		await loginMenu.Login();
	}
}