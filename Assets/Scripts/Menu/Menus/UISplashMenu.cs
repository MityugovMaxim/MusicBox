using System.Threading.Tasks;
using Facebook.Unity;
using MAXHelper;
using UnityEngine;
using Zenject;

[Menu(MenuType.SplashMenu)]
public class UISplashMenu : UIMenu
{
	[SerializeField] UISplashLogo m_Logo;

	[Inject] MenuProcessor m_MenuProcessor;

	protected override async void OnShowFinished()
	{
		await InitializeThirdParty();
		
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

	async Task InitializeThirdParty()
	{
		await InitializeFacebook();
		
		await InitializeGDPR();
		
		await Task.Delay(1500);
		
		await InitializeAppLovin();
	}

	Task InitializeFacebook()
	{
		if (FB.IsInitialized)
			return Task.CompletedTask;
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		FB.Init(
			() =>
			{
				FB.ActivateApp();
				
				completionSource.SetResult(true);
			}
		);
		
		return completionSource.Task;
	}

	Task InitializeGDPR()
	{
		return Task.CompletedTask;
	}

	Task InitializeAppLovin()
	{
		AdsManager.Instance.InitApplovin();
		
		return Task.CompletedTask;
	}
}