using System;
using System.Globalization;
using System.Linq;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
	[SerializeField] Canvas m_Canvas;

	public override void InstallBindings()
	{
		#if UNITY_IOS
		SkAdNetworkBinding.SkAdNetworkUpdateConversionValue(0);
		SkAdNetworkBinding.SkAdNetworkRegisterAppForNetworkAttribution();
		ATTrackingStatusBinding.RequestAuthorizationTracking();
		#endif
		
		Application.targetFrameRate = 60;
		
		InstallCulture();
		
		InstallSignals();
		
		InstallProcessors();
		
		InstallFactories();
		
		InstallAudioManager();
		
		InstallFileManager();
		
		Container.Bind<Canvas>().To<Canvas>().FromInstance(m_Canvas).AsSingle();
		
		Container.Bind<IAdsProvider>().To<AdsProviderMadPixel>().FromNew().AsSingle();
		
		Container.BindInterfacesTo<StatisticUnity>().FromNew().AsSingle();
		Container.BindInterfacesTo<StatisticFirebase>().FromNew().AsSingle();
		Container.BindInterfacesTo<StatisticFacebook>().FromNew().AsSingle();
		Container.BindInterfacesTo<StatisticAppsFlyer>().FromNew().AsSingle();
		Container.BindInterfacesTo<StatisticAppMetrica>().FromNew().AsSingle();
	}

	void InstallCulture()
	{
		SystemLanguage language = Application.systemLanguage;
		
		CultureInfo cultureInfo = CultureInfo
			.GetCultures(CultureTypes.AllCultures)
			.FirstOrDefault(_CultureInfo => _CultureInfo.EnglishName.Equals(language.ToString()));
		
		if (cultureInfo == null)
			return;
		
		cultureInfo = CultureInfo.CreateSpecificCulture(cultureInfo.TwoLetterISOLanguageName);
		
		CultureInfo.CurrentCulture   = cultureInfo;
		CultureInfo.CurrentUICulture = cultureInfo;
	}

	void InstallFactories()
	{
		Container.BindFactory<SongPlayer, SongPlayer, SongPlayer.Factory>().FromFactory<PrefabFactory<SongPlayer>>();
		
		Container.BindFactory<TutorialPlayer, TutorialPlayer, TutorialPlayer.Factory>().FromFactory<PrefabFactory<TutorialPlayer>>();
		
		Container.BindFactory<UIMenu, UIMenu, UIMenu.Factory>().FromFactory<PrefabFactory<UIMenu>>();
		
		Container.BindFactory<UIBackgroundItem, UIBackgroundItem, UIBackgroundItem.Factory>().FromFactory<PrefabFactory<UIBackgroundItem>>();
		
		Container.BindFactory<string, UILatencyElement, UILatencyElement.Factory>().FromFactory<ResourceFactory<UILatencyElement>>();
	}

	void InstallProcessors()
	{
		#if UNITY_IOS
		Container.Bind<MessageProcessor>().To<iOSMessageProcessor>().FromNew().AsSingle();
		#elif UNITY_ANDROID
		Container.Bind<MessageProcessor>().To<AndroidMessageProcessor>().FromNew().AsSingle();
		#endif
		
		InstallProcessor<MenuProcessor>();
		
		InstallProcessor<ScheduleProcessor>();
		InstallProcessor<SocialProcessor>();
		InstallProcessor<ConfigProcessor>();
		InstallProcessor<ApplicationManager>();
		InstallProcessor<HapticProcessor>();
		InstallProcessor<UrlProcessor>();
		InstallProcessor<AdsProcessor>();
		InstallProcessor<StatisticProcessor>();
		InstallProcessor<BannersProcessor>();
		InstallProcessor<LinkProcessor>();
		
		InstallProcessor<HealthController>();
		
		Container.BindInterfacesAndSelfTo<SongController>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<TutorialController>().FromNew().AsSingle();
	}

	void InstallProcessor<T>()
	{
		Container.BindInterfacesAndSelfTo<T>().FromNew().AsSingle();
	}

	void InstallSignals()
	{
		SignalBusInstaller.Install(Container);
	}

	void InstallAudioManager()
	{
		#if UNITY_EDITOR
		Container.Bind(typeof(AudioManager), typeof(IInitializable), typeof(IDisposable)).To<EditorAudioManager>().FromNew().AsSingle();
		#elif UNITY_IOS
		Container.Bind(typeof(AudioManager), typeof(IInitializable), typeof(IDisposable)).To<iOSAudioManager>().FromNew().AsSingle();
		#elif UNITY_ANDROID
		Container.Bind(typeof(AudioManager), typeof(IInitializable), typeof(IDisposable)).To<AndroidAudioManager>().FromNew().AsSingle();
		#endif
	}

	void InstallFileManager()
	{
		#if UNITY_EDITOR
		Container.Bind<IFileManager>().To<EditorFileManager>().FromNew().AsSingle();
		#elif UNITY_IOS
		Container.Bind<IFileManager>().To<iOSFileManager>().FromNew().AsSingle();
		#elif UNITY_ANDROID
		Container.Bind<IFileManager>().To<AndroidFileManager>().FromNew().AsSingle();
		#endif
	}
}
