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

	MaxSdkBase.ConsentDialogState m_ConsentState;

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
		
		await InitializeAppLovin();
		
		await InitializeGDPR();
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

	Task InitializeAppLovin()
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		void Complete(MaxSdkBase.SdkConfiguration _Configuration)
		{
			MaxSdkCallbacks.OnSdkInitializedEvent -= Complete;
			
			m_ConsentState = _Configuration.ConsentDialogState;
			
			completionSource.TrySetResult(true);
		}
		
		MaxSdkCallbacks.OnSdkInitializedEvent += Complete;
		
		MediationManager.Instance.Initialize();
		
		return completionSource.Task;
	}

	async Task InitializeGDPR()
	{
		if (m_ConsentState == MaxSdkBase.ConsentDialogState.DoesNotApply)
			return;
		
		if (m_ConsentState == MaxSdkBase.ConsentDialogState.Unknown && UIConsentMenu.Processed)
		{
			MaxSdk.SetHasUserConsent(true);
			return;
		}
		
		UIConsentMenu consentMenu = m_MenuProcessor.GetMenu<UIConsentMenu>();
		
		if (consentMenu == null)
			return;
		
		await consentMenu.ProcessAsync();
		
		MaxSdk.SetHasUserConsent(true);
	}
}