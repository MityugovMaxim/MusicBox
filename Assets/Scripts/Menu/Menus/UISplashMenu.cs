using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Facebook.Unity;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Android;
using Zenject;

[Menu(MenuType.SplashMenu)]
[SuppressMessage("ReSharper", "AccessToStaticMemberViaDerivedType")]
public class UISplashMenu : UIMenu
{
	[SerializeField] UISplashLogo m_Logo;

	[Inject] MenuProcessor         m_MenuProcessor;
	[Inject] Localization m_Localization;

	MaxSdkBase.ConsentDialogState m_ConsentState;

	protected override async void OnShowFinished()
	{
		base.OnShowFinished();
		
		await ClearCache();
		
		await InitializeThirdParty();
		
		await Task.Delay(250);
		
		await m_Logo.PlayAsync();
		
		await Task.Delay(1500);
		
		await m_MenuProcessor.Show(MenuType.LoginMenu, true);
		
		await Task.WhenAll(
			m_MenuProcessor.Show(MenuType.TransitionMenu),
			m_MenuProcessor.Hide(MenuType.SplashMenu)
		);
		
		UILoginMenu loginMenu = m_MenuProcessor.GetMenu<UILoginMenu>();
		
		await loginMenu.Login();
	}

	protected override void OnHideFinished()
	{
		base.OnHideFinished();
		
		m_MenuProcessor.RemoveMenu(MenuType.SplashMenu);
	}

	static Task ClearCache()
	{
		if (Directory.Exists(Application.temporaryCachePath))
			Directory.Delete(Application.temporaryCachePath, true);
		return Task.CompletedTask;
	}

	async Task InitializeThirdParty()
	{
		await InitializePermissions();
		
		await InitializeFacebook();
		
		await InitializeAppLovin();
		
		await InitializeGDPR();
		
		await InitializeUGS();
	}

	async Task InitializePermissions()
	{
		const string notifications = "android.permission.POST_NOTIFICATIONS";
		
		if (Permission.HasUserAuthorizedPermission(notifications))
			return;
		
		UIPermissionMenu permissionMenu = m_MenuProcessor.GetMenu<UIPermissionMenu>();
		
		permissionMenu.Setup(
			Application.productName,
			m_Localization.Get("PERMISSION_NOTIFICATIONS")
		);
		
		bool state = await permissionMenu.Process(notifications);
		
		if (state)
			Permission.RequestUserPermission(notifications);
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

	async Task InitializeUGS()
	{
		try
		{
			InitializationOptions options = new InitializationOptions().SetEnvironmentName("production");
			
			await UnityServices.InitializeAsync(options);
		}
		catch (Exception)
		{
			// Ignore
		}
	}
}
