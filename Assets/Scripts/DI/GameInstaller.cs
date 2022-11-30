using System;
using System.Globalization;
using System.Linq;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

public class GameInstaller : MonoInstaller
{
	[SerializeField] Canvas m_Canvas;

	[SerializeField] UISocialElement m_SocialElement;

	[SerializeField] UISongHeader  m_SongHeader;
	[SerializeField] UISongItem    m_SongItem;
	[SerializeField] UISongElement m_SongElement;

	[SerializeField] UIProductPromo   m_ProductPromo;
	[SerializeField] UIProductSpecial m_ProductSpecial;
	[SerializeField] UIProductItem    m_ProductItem;
	
	[SerializeField] UIUnlockCoinsItem m_UnlockCoinsItem;
	[SerializeField] UIUnlockSongItem  m_UnlockSongItem;
	[SerializeField] SoundSource       m_SoundSource;

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
		
		InstallManagers();
		
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
		
		InstallPool<UISocialElement, UISocialElement.Pool>(m_SocialElement, 1);
		
		InstallPool<UIProductSpecial, UIProductSpecial.Pool>(m_ProductSpecial, 1);
		InstallPool<UIProductPromo, UIProductPromo.Pool>(m_ProductPromo, 1);
		InstallPool<UIProductItem, UIProductItem.Pool>(m_ProductItem);
		
		InstallPool<UISongHeader, UISongHeader.Pool>(m_SongHeader);
		InstallPool<UISongItem, UISongItem.Pool>(m_SongItem);
		InstallPool<UISongElement, UISongElement.Pool>(m_SongElement);
		
		InstallPool<UIUnlockCoinsItem, UIUnlockCoinsItem.Pool>(m_UnlockCoinsItem, 1);
		InstallPool<UIUnlockSongItem, UIUnlockSongItem.Pool>(m_UnlockSongItem);
		

		InstallPool<SoundSource, SoundSource.Pool>(m_SoundSource);
	}

	void InstallPool<TItem, TPool>(TItem _Prefab, int _Capacity = 5) where TItem : Object where TPool : IMemoryPool
	{
		Container.BindMemoryPool<TItem, TPool>()
			.WithInitialSize(_Capacity)
			.FromComponentInNewPrefab(_Prefab)
			.UnderTransformGroup($"[{typeof(TItem).Name}] Pool");
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
		
		Container.Bind<PreviewProcessor>()
			.To<PreviewProcessor>()
			.FromNewComponentOnNewGameObject()
			.WithGameObjectName("MusicProcessor")
			.UnderTransform(transform)
			.AsSingle();
		
		InstallProcessor<MenuProcessor>();
		
		InstallProcessor<VouchersManager>();
		
		InstallProcessor<VouchersCollection>();
		InstallProcessor<ProductsCollection>();
		InstallProcessor<SongsCollection>();
		
		InstallProcessor<SocialProcessor>();
		InstallProcessor<ConfigProcessor>();
		InstallProcessor<SoundProcessor>();
		InstallProcessor<ApplicationManager>();
		InstallProcessor<HapticProcessor>();
		InstallProcessor<UrlProcessor>();
		InstallProcessor<AdsProcessor>();
		InstallProcessor<StatisticProcessor>();
		InstallProcessor<BannersProcessor>();
		InstallProcessor<StoreProcessor>();
		InstallProcessor<ProgressProcessor>();
		InstallProcessor<RevivesProcessor>();
		InstallProcessor<LinkProcessor>();
		
		InstallProcessor<ProductsDescriptor>();
		
		InstallProcessor<HealthController>();
		InstallProcessor<ScoreController>();
		InstallProcessor<ScoresManager>();
		
		Container.BindInterfacesAndSelfTo<SongController>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<TutorialController>().FromNew().AsSingle();
	}

	void InstallProcessor<T>()
	{
		Container.BindInterfacesAndSelfTo<T>().FromNew().AsSingle();
	}

	void InstallManagers()
	{
		Container.BindInterfacesAndSelfTo<SongsManager>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<ProductsManager>().FromNew().AsSingle();
	}

	void InstallSignals()
	{
		SignalBusInstaller.Install(Container);
		
		Container.DeclareSignal<BannersDataUpdateSignal>().OptionalSubscriber();
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
