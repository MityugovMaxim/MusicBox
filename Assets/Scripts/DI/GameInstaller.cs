using System;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

public class GameInstaller : MonoInstaller
{
	[SerializeField] Canvas            m_Canvas;
	[SerializeField] UISongContainer   m_SongContainer;
	[SerializeField] UISongItem        m_SongItem;
	[SerializeField] UISongGroup       m_SongGroup;
	[SerializeField] UIProductItem     m_ProductItem;
	[SerializeField] UIProductGroup    m_ProductGroup;
	[SerializeField] UIOfferItem       m_OfferItem;
	[SerializeField] UINewsItem        m_NewsItem;
	[SerializeField] UISongUnlockItem  m_SongUnlockItem;
	[SerializeField] UIProductSongItem m_ProductSongItem;
	[SerializeField] UILanguageItem    m_LanguageItem;
	[SerializeField] SoundSource       m_SoundSource;

	public override void InstallBindings()
	{
		Application.targetFrameRate = 60;
		
		InstallCulture();
		
		InstallSignals();
		
		InstallProcessors();
		
		InstallManagers();
		
		InstallFactories();
		
		InstallAudioManager();
		
		InstallFileManager();
		
		Container.Bind<Canvas>().To<Canvas>().FromInstance(m_Canvas).AsSingle();
		Container.BindInterfacesAndSelfTo<UISongContainer>().FromInstance(m_SongContainer).AsSingle();
		
		Container.Bind<IAdsProvider>().To<AdsProviderUnity>().FromNew().AsSingle();
		Container.Bind<IAdsProvider>().To<AdsProviderAdMob>().FromNew().AsSingle();
		
		Container.BindInterfacesTo<StatisticUnity>().FromNew().AsSingle();
		Container.BindInterfacesTo<StatisticFirebase>().FromNew().AsSingle();
		Container.BindInterfacesTo<StatisticFacebook>().FromNew().AsSingle();
		
		InstallPool<UIProductItem, UIProductItem.Pool>(m_ProductItem);
		InstallPool<UIProductGroup, UIProductGroup.Pool>(m_ProductGroup);
		InstallPool<UIProductSongItem, UIProductSongItem.Pool>(m_ProductSongItem);
		InstallPool<UIOfferItem, UIOfferItem.Pool>(m_OfferItem);
		InstallPool<UINewsItem, UINewsItem.Pool>(m_NewsItem);
		InstallPool<UISongItem, UISongItem.Pool>(m_SongItem);
		InstallPool<UISongGroup, UISongGroup.Pool>(m_SongGroup);
		InstallPool<UISongUnlockItem, UISongUnlockItem.Pool>(m_SongUnlockItem);
		InstallPool<UILanguageItem, UILanguageItem.Pool>(m_LanguageItem);
		InstallPool<SoundSource, SoundSource.Pool>(m_SoundSource);
	}

	void InstallPool<TItem, TPool>(TItem _Prefab) where TItem : Object where TPool : IMemoryPool
	{
		Container.BindMemoryPool<TItem, TPool>()
			.WithInitialSize(5)
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
	}

	void InstallProcessors()
	{
		#if UNITY_IOS
		Container.Bind<MessageProcessor>().To<iOSMessageProcessor>().FromNew().AsSingle();
		#elif UNITY_ANDROID
		Container.Bind<MessageProcessor>().To<AndroidMessageProcessor>().FromNew().AsSingle();
		#endif
		
		Container.Bind<MusicProcessor>()
			.To<MusicProcessor>()
			.FromNewComponentOnNewGameObject()
			.WithGameObjectName("MusicProcessor")
			.UnderTransform(transform)
			.AsSingle();
		
		Container.Bind<AmbientProcessor>()
			.To<AmbientProcessor>()
			.FromNewComponentOnNewGameObject()
			.WithGameObjectName("AmbientProcessor")
			.UnderTransform(transform)
			.AsSingle();
		
		InstallProcessor<MenuProcessor>();
		
		InstallProcessor<SocialProcessor>();
		InstallProcessor<ConfigProcessor>();
		InstallProcessor<SoundProcessor>();
		InstallProcessor<ProfileProcessor>();
		InstallProcessor<ApplicationProcessor>();
		InstallProcessor<StorageProcessor>();
		InstallProcessor<LanguageProcessor>();
		InstallProcessor<LocalizationProcessor>();
		InstallProcessor<HapticProcessor>();
		InstallProcessor<UrlProcessor>();
		InstallProcessor<AdsProcessor>();
		InstallProcessor<StatisticProcessor>();
		InstallProcessor<BannersProcessor>();
		InstallProcessor<StoreProcessor>();
		InstallProcessor<NewsProcessor>();
		InstallProcessor<ProductsProcessor>();
		InstallProcessor<SongsProcessor>();
		InstallProcessor<OffersProcessor>();
		InstallProcessor<ProgressProcessor>();
		InstallProcessor<ScoresProcessor>();
		InstallProcessor<RevivesProcessor>();
		
		InstallProcessor<NewsDescriptor>();
		InstallProcessor<OffersDescriptor>();
		InstallProcessor<ProductsDescriptor>();
		
		InstallProcessor<HealthManager>();
		InstallProcessor<ScoreManager>();
		
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
		Container.BindInterfacesAndSelfTo<OffersManager>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<ProductsManager>().FromNew().AsSingle();
	}

	void InstallSignals()
	{
		SignalBusInstaller.Install(Container);
		
		Container.DeclareSignal<BannersDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<LanguageDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<LanguageSelectSignal>().OptionalSubscriber();
		Container.DeclareSignal<ApplicationDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<SocialDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<ProfileDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<SongsDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<ScoresDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<ProductsDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<StoreDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<NewsDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<OffersDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<ProgressDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<RevivesDataUpdateSignal>().OptionalSubscriber();
		
		Container.DeclareSignal<InputMissSignal>().OptionalSubscriber();
		
		Container.DeclareSignal<TapSuccessSignal>().OptionalSubscriber();
		Container.DeclareSignal<TapFailSignal>().OptionalSubscriber();
		
		Container.DeclareSignal<DoubleSuccessSignal>().OptionalSubscriber();
		Container.DeclareSignal<DoubleFailSignal>().OptionalSubscriber();
		
		Container.DeclareSignal<HoldHitSignal>().OptionalSubscriber();
		Container.DeclareSignal<HoldMissSignal>().OptionalSubscriber();
		Container.DeclareSignal<HoldSuccessSignal>().OptionalSubscriber();
		Container.DeclareSignal<HoldFailSignal>().OptionalSubscriber();
		
		Container.DeclareSignal<ScoreSignal>().OptionalSubscriber();
		Container.DeclareSignal<HealthSignal>().OptionalSubscriber();
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
		
		Container.DeclareSignal<AudioSourceChangedSignal>().OptionalSubscriber();
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