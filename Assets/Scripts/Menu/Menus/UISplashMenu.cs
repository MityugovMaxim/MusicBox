using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[Menu(MenuType.SplashMenu)]
public class UISplashMenu : UIMenu
{
	const string SETUP_KEY = "SETUP";

	static int SetupCount
	{
		get => PlayerPrefs.GetInt(SETUP_KEY, 0);
		set => PlayerPrefs.SetInt(SETUP_KEY, value);
	}

	[SerializeField] UISplashLogo m_Logo;

	[Inject] MenuProcessor m_MenuProcessor;

	protected override async void OnShowFinished()
	{
		await Task.Delay(250);
		
		await m_Logo.PlayAsync();
		
		await Task.Delay(1500);
		
		if (SetupCount == 0)
		{
			SetupCount = 1;
			await m_MenuProcessor.Show(MenuType.SetupMenu, true);
			await m_MenuProcessor.Hide(MenuType.SplashMenu);
		}
		else
		{
			await m_MenuProcessor.Show(MenuType.LoginMenu, true);
			await m_MenuProcessor.Hide(MenuType.SplashMenu);
			
			UILoginMenu loginMenu = m_MenuProcessor.GetMenu<UILoginMenu>();
			
			await loginMenu.Login();
		}
	}

	protected override void OnHideFinished()
	{
		m_MenuProcessor.RemoveMenu(MenuType.SplashMenu);
	}
}