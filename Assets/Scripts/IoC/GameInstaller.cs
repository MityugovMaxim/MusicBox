using System;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
	[SerializeField] Canvas      m_Canvas;
	[SerializeField] ProductInfo m_NoAdsProduct;

	public override void InstallBindings()
	{
		InstallCulture();
		
		InstallSignals();
		
		InstallProcessors();
		
		InstallFactories();
		
		InstallAudioManager();
		
		Container.Bind<Canvas>().To<Canvas>().FromInstance(m_Canvas);
		Container.Bind<ProductInfo>().FromScriptableObject(m_NoAdsProduct).AsSingle();
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
		Container.BindFactory<string, Action<Level>, ResourceRequest, Level.Factory>().FromFactory<AsyncPrefabResourceFactory<Level>>();
		
		Container.BindFactory<UIMainMenuItem, UIMainMenuItem, UIMainMenuItem.Factory>().FromFactory<PrefabFactory<UIMainMenuItem>>();
		
		Container.BindFactory<UIShopMenuItem, UIShopMenuItem, UIShopMenuItem.Factory>().FromFactory<PrefabFactory<UIShopMenuItem>>();
		
		Container.BindFactory<UIProductMenuItem, UIProductMenuItem, UIProductMenuItem.Factory>().FromFactory<PrefabFactory<UIProductMenuItem>>();
		
		Container.BindFactory<UIMenu, UIMenu, UIMenu.Factory>().FromFactory<PrefabFactory<UIMenu>>();
	}

	void InstallProcessors()
	{
		#if UNITY_IOS
		Container.Bind(typeof(AdsProcessor), typeof(IInitializable)).To<iOSAdsProcessor>().FromNew().AsSingle();
		#endif
		
		Container.BindInterfacesAndSelfTo<MenuProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<HapticProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<SocialProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<PurchaseProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<ScoreProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<LevelProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<StatisticProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<ProgressProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<NotificationProcessor>().FromNew().AsSingle();
		Container.BindInterfacesAndSelfTo<ConfigProcessor>().FromNew().AsSingle();
	}

	void InstallSignals()
	{
		SignalBusInstaller.Install(Container);
		
		Container.DeclareSignal<PurchaseSignal>();
		Container.DeclareSignal<ConfigSignal>();
		
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