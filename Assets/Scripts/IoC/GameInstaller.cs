using System.Globalization;
using System.Linq;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
	[SerializeField] Canvas m_Canvas;

	[SerializeField] UILevelItem   m_LevelItem;
	[SerializeField] UILevelGroup  m_LevelGroup;
	[SerializeField] UIStoreItem   m_StoreItem;
	[SerializeField] UIOfferItem   m_OfferItem;
	[SerializeField] UINewsItem    m_NewsItem;
	[SerializeField] UIUnlockItem  m_UnlockItem;
	[SerializeField] UIProductItem m_ProductItem;

	public override void InstallBindings()
	{
		InstallCulture();
		
		InstallSignals();
		
		InstallProcessors();
		
		InstallManagers();
		
		InstallFactories();
		
		InstallAudioManager();
		
		Container.Bind<Canvas>().To<Canvas>().FromInstance(m_Canvas);
		
		Container.Bind<IAdsProvider>().To<AdsProviderUnity>().FromNew().AsSingle();
		Container.Bind<IAdsProvider>().To<AdsProviderAdMob>().FromNew().AsSingle();
		
		Container.BindInterfacesTo<StatisticUnity>().FromNew().AsSingle();
		Container.BindInterfacesTo<StatisticFirebase>().FromNew().AsSingle();
		Container.BindInterfacesTo<StatisticFacebook>().FromNew().AsSingle();
		
		Container.BindMemoryPool<UILevelItem, UILevelItem.Pool>()
			.WithInitialSize(10)
			.FromComponentInNewPrefab(m_LevelItem)
			.UnderTransformGroup("[UIMainMenuItem] Pool");
		
		Container.BindMemoryPool<UILevelGroup, UILevelGroup.Pool>()
			.WithInitialSize(3)
			.FromComponentInNewPrefab(m_LevelGroup)
			.UnderTransformGroup("[UIMainMenuGroup] Pool");
		
		Container.BindMemoryPool<UIStoreItem, UIStoreItem.Pool>()
			.WithInitialSize(5)
			.FromComponentInNewPrefab(m_StoreItem)
			.UnderTransformGroup("[UIShopMenuItem] Pool");
		
		Container.BindMemoryPool<UIOfferItem, UIOfferItem.Pool>()
			.WithInitialSize(5)
			.FromComponentInNewPrefab(m_OfferItem)
			.UnderTransformGroup("[UIOfferMenuItem] Pool");
		
		Container.BindMemoryPool<UINewsItem, UINewsItem.Pool>()
			.WithInitialSize(5)
			.FromComponentInNewPrefab(m_NewsItem)
			.UnderTransformGroup("[UINewsMenuItem] Pool");
		
		Container.BindMemoryPool<UIUnlockItem, UIUnlockItem.Pool>()
			.WithInitialSize(5)
			.FromComponentInNewPrefab(m_UnlockItem)
			.UnderTransformGroup("[UIUnlockItem] Pool");
		
		Container.BindMemoryPool<UIProductItem, UIProductItem.Pool>()
			.WithInitialSize(5)
			.FromComponentInNewPrefab(m_ProductItem)
			.UnderTransformGroup("[UIProductItem] Pool");
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
		Container.BindFactory<Level, Level, Level.Factory>().FromFactory<PrefabFactory<Level>>();
		
		Container.BindFactory<UIMenu, UIMenu, UIMenu.Factory>().FromFactory<PrefabFactory<UIMenu>>();
	}

	void InstallProcessors()
	{
		#if UNITY_IOS
		Container.Bind(typeof(MessageProcessor), typeof(IInitializable)).To<iOSMessageProcessor>().FromNew().AsSingle();
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
		
		Container.BindInterfacesAndSelfTo<ApplicationProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<LanguageProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<AdsProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<TimeProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<UrlProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<SocialProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<StorageProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<ProductProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<StoreProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<LevelProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<HealthProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<ScoreProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<NewsProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<OffersProcessor>().FromNew().AsSingle();
		
		Container.BindInterfacesAndSelfTo<HapticProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<StatisticProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<ProgressProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<ProfileProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<MenuProcessor>().FromNew().AsSingle();
		
		Container.BindInterfacesAndSelfTo<LevelController>().FromNew().AsSingle();
	}

	void InstallManagers()
	{
		Container.BindInterfacesAndSelfTo<LevelManager>().FromNew().AsSingle();
	}

	void InstallSignals()
	{
		SignalBusInstaller.Install(Container);
		
		Container.DeclareSignal<ApplicationDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<SocialDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<ProfileDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<LevelDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<ScoreDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<ProductDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<StoreDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<NewsDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<OfferDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<ProgressDataUpdateSignal>().OptionalSubscriber();
		
		Container.DeclareSignal<LevelStartSignal>();
		Container.DeclareSignal<LevelPlaySignal>();
		Container.DeclareSignal<LevelRestartSignal>();
		Container.DeclareSignal<LevelExitSignal>();
		Container.DeclareSignal<LevelFinishSignal>();
		Container.DeclareSignal<LevelUnlockSignal>();
		Container.DeclareSignal<LevelScoreSignal>();
		Container.DeclareSignal<LevelComboSignal>();
		
		Container.DeclareSignal<HoldHitSignal>();
		Container.DeclareSignal<HoldMissSignal>();
		Container.DeclareSignal<HoldSuccessSignal>();
		Container.DeclareSignal<HoldFailSignal>();
		
		Container.DeclareSignal<TapSuccessSignal>();
		Container.DeclareSignal<TapFailSignal>();
		
		Container.DeclareSignal<DoubleSuccessSignal>();
		Container.DeclareSignal<DoubleFailSignal>();
		
		Container.DeclareSignal<HealthChangedSignal>();
	}

	void InstallAudioManager()
	{
		Container.BindInterfacesAndSelfTo<AudioManager>().FromNew().AsSingle();
		Container.DeclareSignal<AudioPlaySignal>();
		Container.DeclareSignal<AudioPauseSignal>();
		Container.DeclareSignal<AudioNextTrackSignal>();
		Container.DeclareSignal<AudioPreviousTrackSignal>();
		Container.DeclareSignal<AudioSourceChangedSignal>();
	}
}