using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.SplashMenu)]
public class UISplashMenu : UIMenu
{
	[SerializeField] UISplashLogo m_Logo;

	[Inject] MenuProcessor m_MenuProcessor;

	protected override async void OnShowFinished()
	{
		await Task.Delay(250);
		
		await m_Logo.PlayAsync();
		
		await Task.Delay(1500);
		
		await m_MenuProcessor.Show(MenuType.LoginMenu, true);
		await m_MenuProcessor.Hide(MenuType.SplashMenu);
		
		UILoginMenu loginMenu = m_MenuProcessor.GetMenu<UILoginMenu>();
		
		await loginMenu.Login();
	}

	protected override void OnHideFinished()
	{
		m_MenuProcessor.RemoveMenu(MenuType.SplashMenu);
	}
}