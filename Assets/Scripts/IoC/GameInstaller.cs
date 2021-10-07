using System.Globalization;
using System.Linq;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
	[SerializeField] Canvas         m_Canvas;
	[SerializeField] ProductInfo    m_NoAdsProduct;
	[SerializeField] MusicProcessor m_MusicProcessor;

	[SerializeField] UILevelsPageItem  m_LevelsPageItem;
	[SerializeField] UIMainMenuGroup m_MainMenuGroup;
	[SerializeField] UIStorePageItem  m_StorePageItem;
	[SerializeField] UIOffersPageItem m_OffersPageItem;
	[SerializeField] UINewsPageItem  m_NewsPageItem;

	public override void InstallBindings()
	{
		InstallCulture();
		
		InstallSignals();
		
		InstallProcessors();
		
		InstallFactories();
		
		InstallAudioManager();
		
		Container.Bind<Canvas>().To<Canvas>().FromInstance(m_Canvas);
		Container.Bind<ProductInfo>().FromScriptableObject(m_NoAdsProduct).AsSingle();
		
		Container.BindMemoryPool<UILevelsPageItem, UILevelsPageItem.Pool>()
			.WithInitialSize(10)
			.FromComponentInNewPrefab(m_LevelsPageItem)
			.UnderTransformGroup("[UIMainMenuItem] Pool");
		
		Container.BindMemoryPool<UIMainMenuGroup, UIMainMenuGroup.Pool>()
			.WithInitialSize(3)
			.FromComponentInNewPrefab(m_MainMenuGroup)
			.UnderTransformGroup("[UIMainMenuGroup] Pool");
		
		Container.BindMemoryPool<UIStorePageItem, UIStorePageItem.Pool>()
			.WithInitialSize(5)
			.FromComponentInNewPrefab(m_StorePageItem)
			.UnderTransformGroup("[UIShopMenuItem] Pool");
		
		Container.BindMemoryPool<UIOffersPageItem, UIOffersPageItem.Pool>()
			.WithInitialSize(5)
			.FromComponentInNewPrefab(m_OffersPageItem)
			.UnderTransformGroup("[UIOfferMenuItem] Pool");
		
		Container.BindMemoryPool<UINewsPageItem, UINewsPageItem.Pool>()
			.WithInitialSize(5)
			.FromComponentInNewPrefab(m_NewsPageItem)
			.UnderTransformGroup("[UINewsMenuItem] Pool");
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
		
		Container.BindFactory<UIProductMenuItem, UIProductMenuItem, UIProductMenuItem.Factory>().FromFactory<PrefabFactory<UIProductMenuItem>>();
		
		Container.BindFactory<UIMenu, UIMenu, UIMenu.Factory>().FromFactory<PrefabFactory<UIMenu>>();
	}

	void InstallProcessors()
	{
		#if UNITY_IOS
		Container.Bind(typeof(AdsProcessor), typeof(IInitializable)).To<iOSAdsProcessor>().FromNew().AsSingle();
		#endif
		
		Container.Bind<MusicProcessor>().To<MusicProcessor>().FromInstance(m_MusicProcessor).AsSingle();
		
		Container.BindInterfacesAndSelfTo<UrlProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<SocialProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<StorageProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<StoreProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<LevelProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<ScoreProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<MessageProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<NewsProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<OfferProcessor>().FromNew().AsSingle();
		
		Container.BindInterfacesAndSelfTo<HapticProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<StatisticProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<ProfileProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<MenuProcessor>().FromNew().AsSingle();
	}

	void InstallSignals()
	{
		SignalBusInstaller.Install(Container);
		
		Container.DeclareSignal<LoginSignal>().OptionalSubscriber();
		Container.DeclareSignal<LogoutSignal>().OptionalSubscriber();
		
		Container.DeclareSignal<PurchaseSignal>();
		
		Container.DeclareSignal<ProfileDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<LevelDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<ScoreDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<ProductDataUpdateSignal>().OptionalSubscriber();
		Container.DeclareSignal<PurchaseDataUpdateSignal>().OptionalSubscriber();
		
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